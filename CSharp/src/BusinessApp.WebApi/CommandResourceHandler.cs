namespace BusinessApp.WebApi
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System;

    /// <summary>
    /// Basic http handler for commands
    /// </summary>
    public class CommandResourceHandler<TRequest> : IHttpRequestHandler<TRequest, TRequest>
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

        public async Task<Result<TRequest, IFormattable>> HandleAsync(HttpContext context, CancellationToken cancellationToken)
        {
            var command = await context.DeserializeIntoAsync<TRequest>(serializer, cancellationToken);

            if (command == null)
            {
                return Result<TRequest, IFormattable>
                    .Error(new ModelValidationException("Your requets was cancelled because there was no data to process"));
            }

            return await handler.HandleAsync(command, cancellationToken);
        }
    }
}
