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
    public class ValidationBatchCommandDecorator<TRequest, TResponse>
        : IRequestHandler<IEnumerable<TRequest>, TResponse>
    {
        private readonly IValidator<TRequest> singleValidator;
        private readonly IRequestHandler<IEnumerable<TRequest>, TResponse> handler;

        public ValidationBatchCommandDecorator(IValidator<TRequest> singleValidator,
            IRequestHandler<IEnumerable<TRequest>, TResponse> handler)
        {
            this.singleValidator = Guard.Against.Null(singleValidator).Expect(nameof(singleValidator));
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
        }

        public async Task<Result<TResponse, IFormattable>> HandleAsync(
            IEnumerable<TRequest> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var results = new List<Result<IEnumerable<TRequest>, IFormattable>>();

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
                        results.Add(Result<IEnumerable<TRequest>, IFormattable>.Error(invalid));
                    }
                }
                catch (ModelValidationException ex)
                {
                    results.Add(Result<IEnumerable<TRequest>, IFormattable>.Error(ex));
                }
            }

            if (results.Any(r => r.Kind == Result.Error))
            {
                return Result<TResponse, IFormattable>
                    .Error(new BatchException(
                        results.Select(o => o.IgnoreValue()
                    )));
            }

            return await handler.HandleAsync(command, cancellationToken);
        }
    }
}
