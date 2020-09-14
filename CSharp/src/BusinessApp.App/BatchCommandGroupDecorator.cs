namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessApp.Domain;

    public class BatchCommandGroupDecorator<TCommand> : ICommandHandler<IEnumerable<TCommand>>
    {
        private static readonly Regex regex = new Regex(@"^\[(\d+)\](\..*)$");
        private readonly IBatchGrouper<TCommand> grouper;
        private readonly ICommandHandler<IEnumerable<TCommand>> handler;

        public BatchCommandGroupDecorator(
            IBatchGrouper<TCommand> grouper,
            ICommandHandler<IEnumerable<TCommand>> handler)
        {
            this.grouper = GuardAgainst.Null(grouper, nameof(grouper));
            this.handler = GuardAgainst.Null(handler, nameof(handler));
        }

        public async Task HandleAsync(IEnumerable<TCommand> command,
            CancellationToken cancellationToken)
        {
            GuardAgainst.Null(command, nameof(command));

            var payloads = await grouper.GroupAsync(command, cancellationToken);

            var errors = new List<Exception>();
            var tasks = new List<Task>();

            // use Parallel so that work is batched into a smaller # of tasks
            // https://stackoverflow.com/questions/19102966/parallel-foreach-vs-task-run-and-task-whenall
            await Task.Run(() => Parallel.ForEach(payloads, async payload =>
            {
                try
                {
                    await handler.HandleAsync(payload, cancellationToken);
                }
                catch (AggregateException ex)
                {
                    errors.Add(ex);

                    foreach (var e in ex.InnerExceptions)
                    {
                        FindAndChangeIndexKey(e, payload, command);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);

                    foreach (var e in ex.Flatten())
                    {
                        FindAndChangeIndexKey(e, payload, command);
                    }
                }
            }));

            await Task.WhenAll(tasks);

            if (errors.Count == 1)
            {
                throw errors.First();
            }
            else if (errors.Count > 1)
            {
                throw new AggregateException(errors);
            }
        }

        private static void FindAndChangeIndexKey(Exception e, IEnumerable<TCommand> payload,
            IEnumerable<TCommand> originalCommands)
        {
            var wrongIndex = FindIndex(e);

            if (wrongIndex != -1)
            {
                var rightIndex = NormalizeIndex(payload, originalCommands, wrongIndex);

                if (wrongIndex != rightIndex)
                {
                    ChangePayloadIndex(rightIndex, e);
                }
            }
        }

        private static int FindIndex(Exception e)
        {
            if (e.Data.Contains("Index"))
            {
                // oh why thank you
                if (int.TryParse(e.Data["Index"].ToString(), out int wrongIndex))
                {
                    return wrongIndex;
                }
            }

            // AAAh! what have you done!
            foreach (var key in e.Data.Keys)
            {
                var match = regex.Match(key.ToString());

                if (match.Success)
                {
                    var indexMaybe = match.Groups[1].Value;

                    if (int.TryParse(indexMaybe, out int wrongIndex))
                    {
                        return wrongIndex;
                    }
                }
            }

            return -1;
        }

        private static int NormalizeIndex(IEnumerable<TCommand> payload, IEnumerable<TCommand> original, int payloadIndex)
        {
            var payloadCommand = payload.ElementAt(payloadIndex);

            return original.Select((o, i) => new { o, i })
                .FirstOrDefault(p => ReferenceEquals(p.o, payloadCommand))
                ?.i ?? -1;
        }

        private static void ChangePayloadIndex(int index, Exception e)
        {
            var wrongKeyMap = new Dictionary<object, string>();

            foreach (var key in e.Data.Keys)
            {
                var keyAsString = key.ToString();

                if (regex.Match(keyAsString).Success)
                {
                    wrongKeyMap.Add(key,
                        regex.Replace(keyAsString, m => $"[{index}]{m.Groups[2]}"));
                }
                else
                {
                    wrongKeyMap.Add(key, $"[{index}].{key}");
                }
            }

            foreach (var kvp in wrongKeyMap)
            {
                e.Data.Add(kvp.Value, e.Data[kvp.Key]);
                e.Data.Remove(kvp.Key);
            }
        }
    }
}
