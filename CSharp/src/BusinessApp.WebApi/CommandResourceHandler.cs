namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;

    /// <summary>
    /// Basic http handler for commands
    /// </summary>
    public class CommandResourceHandler<TRequest> : IResourceHandler<TRequest, TRequest>
        where TRequest : class, new()
    {
        private readonly ICommandHandler<TRequest> handler;
        private readonly ISerializer serializer;

        public CommandResourceHandler(
            ICommandHandler<TRequest> handler,
            ISerializer serializer
        )
        {
            this.handler = Guard.Against.Null(handler).Expect(nameof(handler));
            this.serializer = Guard.Against.Null(serializer).Expect(nameof(serializer));
        }

        public async Task<TRequest> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var command = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (command == null)
            {
                throw new ValidationException("No Data found");
            }

            await handler.HandleAsync(command, cancellationToken);

            return command;
        }
    }
}
