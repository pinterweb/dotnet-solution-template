namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandHandler<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private readonly ICommandHandler<TCommand> inner;

        public BatchCommandHandler(ICommandHandler<TCommand> inner)
        {
            this.inner = GuardAgainst.Null(inner, nameof(inner));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            var errors = new List<Exception>();

            for (int i = 0; i < command.Count(); i++)
            {
                var c = command.ElementAt(i);

                try
                {
                    await inner.HandleAsync(c, cancellationToken);
                }
                catch (ValidationException ex)
                {
                    var indexResult = ex.Result.CreateWithIndexName(i);

                    errors.Add(new ValidationException(indexResult, ex.InnerException));
                }
                catch (SecurityResourceException ex)
                {
                    var indexResult = ex.ResourceName.CreateIndexName(i);

                    errors.Add(
                        new SecurityResourceException(indexResult, ex.Message, ex.InnerException));
                }
            }

            if (errors.Count == 1)
            {
                throw errors.First();
            }
            else if (errors.Count() > 1)
            {
                throw new AggregateException(errors);
            }
        }
    }
}
