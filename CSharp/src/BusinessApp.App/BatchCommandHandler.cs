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
            this.inner = Guard.Against.Null(inner).Expect(nameof(inner));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(command).Expect(nameof(command));

            var errors = new List<Exception>();

            for (int i = 0; i < command.Count(); i++)
            {
                var c = command.ElementAt(i);

                try
                {
                    await inner.HandleAsync(c, cancellationToken);
                }
                catch(Exception ex)
                {
                    foreach (var e in ex.Flatten())
                    {
                        IndexException(i, e);
                    }

                    errors.Add(ex);
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

        private static void IndexException(int commandIndex, Exception ex)
        {
            var newKeys = new Dictionary<object, string>();

            foreach (var key in ex.Data.Keys)
            {
                var indexKey = key.ToString().CreateIndexName(commandIndex);
                newKeys.Add(key, indexKey);
            }

            foreach (var kvp in newKeys)
            {
                ex.Data.Add(kvp.Value, ex.Data[kvp.Key]);
                ex.Data.Remove(kvp.Key);
            }

            ex.Data.Add("Index", commandIndex);
        }
    }
}
