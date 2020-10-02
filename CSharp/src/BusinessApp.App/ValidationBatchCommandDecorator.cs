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

        public async Task<Result<IEnumerable<TCommand>, IFormattable>> HandleAsync(
            IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var results = new List<Result<IEnumerable<TCommand>, IFormattable>>();

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
                        .Where(e => e is ModelValidationException)
                        .Cast<ModelValidationException>();

                    foreach (var invalid in invalids)
                    {
                        results.Add(Result<IEnumerable<TCommand>, IFormattable>.Error(invalid));
                    }
                }
                catch (ModelValidationException ex)
                {
                    results.Add(Result<IEnumerable<TCommand>, IFormattable>.Error(ex));
                }
            }

            if (results.Any(r => r.Kind == Result.Error))
            {
                return Result<IEnumerable<TCommand>, IFormattable>
                    .Error(new BatchException(
                        results.Select(o => o.IgnoreValue()
                    )));
            }

            return await handler.HandleAsync(command, cancellationToken);
        }
    }
}
