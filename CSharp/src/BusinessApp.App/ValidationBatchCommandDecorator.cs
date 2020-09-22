namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    /// <summary>
    /// Validates each command in the batch
    /// </summary>
    public class ValidationBatchCommandDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly IValidator<TCommand> singleValidator;
        private readonly ICommandHandler<IEnumerable<TCommand>> handler;

        public ValidationBatchCommandDecorator(IValidator<TCommand> singleValidator,
            ICommandHandler<IEnumerable<TCommand>> handler)
        {
            this.singleValidator = Guard.Against.Null(singleValidator).Expect(nameof(singleValidator));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));
            var errors = new List<ValidationException>();

            for (int i = 0; i < command.Count(); i++)
            {
                var c = command.ElementAt(i);

                try
                {
                    await singleValidator.ValidateAsync(c, cancellationToken);
                }
                catch (AggregateException ex)
                {
                    var invalids = ex.Flatten().InnerExceptions
                        .Where(e => e is ValidationException)
                        .Cast<ValidationException>();

                    foreach (var invalid in invalids)
                    {
                        AddError(invalid, i, errors);
                    }
                }
                catch (ValidationException ex)
                {
                    AddError(ex, i, errors);
                }
            }

            if (errors.Count == 1)
            {
                throw errors.First();
            }
            else if (errors.Count > 1)
            {
                throw new AggregateException(errors);
            }

            await handler.HandleAsync(command, cancellationToken);
        }

        private static void AddError(ValidationException ex, int index,
            List<ValidationException> errors)
        {
            var indexResult = ex.Result.CreateWithIndexName(index);

            errors.Add(new ValidationException(indexResult, ex.InnerException));
        }
    }
}
