namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using System.Collections.Generic;
#if efcore
    using BusinessApp.Data;
#endif

    /// <summary>
    /// Allows registering all types that are defined in the app layer
    /// </summary>
    public static class AppLayerBootstrapper
    {
        public static readonly Assembly Assembly = typeof(IQuery<>).Assembly;

        public static void Bootstrap(Container container,
            IWebHostEnvironment env,
            BootstrapOptions options)
        {
            Guard.Against.Null(container).Expect(nameof(container));

            container.Collection.Register(typeof(IValidator<>), new[] { Assembly });

            container.Register(typeof(IBatchMacro<,>), Assembly);

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly);
            container.Collection.Append(typeof(IValidator<>), typeof(FluentValidationValidator<>));
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));

            container.Collection.Append(typeof(IValidator<>), typeof(DataAnnotationsValidator<>));
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

            container.Register(typeof(IBatchGrouper<>), Assembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);

            container.RegisterLoggers(env, options);

            var handlerTypes = container.GetTypesToRegister(typeof(IRequestHandler<,>),
                new[] { Assembly },
                new TypesToRegisterOptions
                {
                    IncludeGenericTypeDefinitions = true,
                    IncludeComposites = false,
                });

            foreach (var handlerType in handlerTypes) container.Register(handlerType);

            // XXX Order of decorator registration matters.
            // First decorator wraps the real instance
            #region Decoration Registration

            container.RegisterQueryDecorator(typeof(QueryLifetimeCacheDecorator<,>));
#if efcore
                        container.RegisterQueryDecorator(typeof(EFTrackingQueryDecorator<,>));
#endif
            container.RegisterQueryDecorator(typeof(EntityNotFoundQueryDecorator<,>));

            container.RegisterCommandDecorator(typeof(TransactionDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(DeadlockRetryDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(ApplicationScopeBatchDecorator<,>),
                lifestyle: Lifestyle.Singleton);

            container.RegisterCommandDecorator(typeof(BatchCommandGroupDecorator<,>));

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(ValidationRequestDecorator<,>));

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(AuthorizationRequestDecorator<,>),
                ctx =>
                {
                    var requestType = ctx.ServiceType.GetGenericArguments()[0];

                    bool HasAuthAttribute(Type targetType)
                    {
                        return targetType
                              .GetCustomAttributes(typeof(AuthorizeAttribute))
                              .Any();
                    }

                    while(!HasAuthAttribute(requestType) && requestType.IsConstructedGenericType)
                    {
                        requestType = requestType.GetGenericArguments()[0];
                    }

                    return HasAuthAttribute(requestType) && IsOuterScope(ctx);
                });

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(RequestExceptionDecorator<,>),
                ctx => IsOuterScope(ctx));

            #endregion

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroScopeWrappingHandler<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(BatchMacroCommandDecorator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchCommandHandler<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() != typeof(BatchMacroCommandDecorator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchMacroCommandDecorator<,,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c =>
                {
                    var requestType = c.ServiceType.GetGenericArguments()[0];
                    var responseType = c.ServiceType.GetGenericArguments()[1];

                    var handler =
                        handlerTypes.FirstOrDefault(t => t.GetInterfaces().Any(i => i == c.ServiceType)) ??
                        handlerTypes.Where(h => h.GetGenericArguments().Length == 2)
                            .FirstOrDefault(t => t.MakeGenericType(requestType, responseType).GetInterfaces().Any(i => i == c.ServiceType));

                    if (handler == null)
                    {
                        throw new BusinessAppWebApiException(
                            $"No command handler is setup for command '{requestType.Name}'. Please set one up.");
                    }
                    else
                    {
                        return c.Consumer.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<,>)
                            ? typeof(BatchScopeWrappingHandler<,,>).MakeGenericType(handler, requestType, responseType)
                            : handler;
                    }
                },
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private static bool HasTransactionScope(DecoratorPredicateContext ctx)
        {
            return !ctx.ImplementationType.IsConstructedGenericType ||
            (
                ctx.ImplementationType.GetGenericTypeDefinition() == typeof(BatchCommandHandler<,>) ||
                ctx.ImplementationType.GetGenericTypeDefinition() == typeof(MacroScopeWrappingHandler<,>)
            );
        }

        private static bool IsOuterScope(DecoratorPredicateContext ctx)
        {
            return !ctx.ImplementationType.IsConstructedGenericType ||
            (
                ctx.ImplementationType.GetGenericTypeDefinition() != typeof(BatchScopeWrappingHandler<,,>) &&
                ctx.ImplementationType.GetGenericTypeDefinition() != typeof(MacroScopeWrappingHandler<,>)
            );
        }


        public sealed class MacroScopeWrappingHandler<TRequest, TResponse> :
            BatchScopeWrappingHandler<BatchCommandHandler<TRequest, TResponse>, IEnumerable<TRequest>, IEnumerable<TResponse>>
        {
            public MacroScopeWrappingHandler(BatchCommandHandler<TRequest, TResponse> inner)
                : base(inner)
            {
            }
        }

        public class BatchScopeWrappingHandler<TConsumer, TRequest, TResponse> :
            DefaultScopeWrappingHandler<TConsumer, TRequest, TResponse>
            where TConsumer : IRequestHandler<TRequest, TResponse>
        {
            public BatchScopeWrappingHandler(TConsumer inner) : base(inner)
            {
            }
        }

        public class DefaultScopeWrappingHandler<TConsumer, TRequest, TResponse> :
            IRequestHandler<TRequest, TResponse>
            where TConsumer : IRequestHandler<TRequest, TResponse>
        {
            private readonly TConsumer inner;

            public DefaultScopeWrappingHandler(TConsumer inner)
            {
                this.inner = inner;
            }

            public Task<Result<TResponse, IFormattable>> HandleAsync(TRequest command,
                CancellationToken cancellationToken)
            {
                return inner.HandleAsync(command, cancellationToken);
            }
        }
    }
}
