﻿namespace BusinessApp.WebApi
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
        public static void Bootstrap(Container container,
            IWebHostEnvironment env,
            BootstrapOptions options)
        {
            Guard.Against.Null(container).Expect(nameof(container));

            var registrations = new[] { typeof(DataAnnotationsValidator<>) }.Concat(container.GetTypesToRegister(
                typeof(IValidator<>),
                new[] { options.AppLayerAssembly }
            ));
            container.Collection.Register(typeof(IValidator<>), registrations);
            container.Register(typeof(IValidator<>), typeof(CompositeValidator<>), Lifestyle.Singleton);

#if fluentvalidation
            container.Collection.Register(typeof(FluentValidation.IValidator<>), options.AppLayerAssembly);
            container.Collection.Append(typeof(IValidator<>), typeof(FluentValidationValidator<>));
#endif
            container.Register(typeof(IAuthorizer<>), typeof(AuthorizeAttributeHandler<>));

#if batch
            container.Register(typeof(IBatchGrouper<>), options.AppLayerAssembly);
            container.Register(typeof(IBatchMacro<,>), options.AppLayerAssembly);
            container.RegisterConditional(typeof(IBatchGrouper<>),
                typeof(NullBatchGrouper<>),
                ctx => !ctx.Handled);
#endif

            container.RegisterLoggers(env, options);

            IEnumerable<Type> GetTypesToRegister()
            {
                return container.GetTypesToRegister(typeof(IRequestHandler<,>),
                    new[] { options.AppLayerAssembly },
                    new TypesToRegisterOptions
                    {
                        IncludeGenericTypeDefinitions = true,
                        IncludeComposites = false,
                    });
            };

            foreach (var handlerType in GetTypesToRegister()) container.Register(handlerType);

            // XXX Order of decorator registration matters.
            // First decorator wraps the real instance
            #region Decoration Registration

            container.RegisterQueryDecorator(typeof(InstanceCacheQueryDecorator<,>));
            container.RegisterQueryDecorator(typeof(EntityNotFoundQueryDecorator<,>));

            container.RegisterCommandDecorator(typeof(TransactionRequestDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(DeadlockRetryRequestDecorator<,>),
                ctx => HasTransactionScope(ctx));

            container.RegisterCommandDecorator(typeof(ScopedBatchRequestProxy<,>),
                lifestyle: Lifestyle.Singleton);

            container.RegisterCommandDecorator(typeof(GroupedBatchRequestDecorator<,>));

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(ValidationRequestDecorator<,>));

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(AuthorizationRequestDecorator<,>),
                ctx =>
                {
                    var requestType = ctx.ServiceType.GetGenericArguments()[0];

                    static bool HasAuthAttribute(Type targetType)
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

#if efcore
            container.RegisterQueryDecorator(typeof(EFTrackingQueryDecorator<,>));
#endif
            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(RequestExceptionDecorator<,>),
                ctx => IsOuterScope(ctx));

            #endregion

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroScopeWrappingHandler<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(MacroBatchRequestDelegator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(BatchRequestDelegator<,>),
                ctx => ctx.Consumer?.ImplementationType.GetGenericTypeDefinition() != typeof(MacroBatchRequestDelegator<,,>));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(MacroBatchRequestDelegator<,,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(typeof(IRequestHandler<,>),
                c =>
                {
                    var requestType = c.ServiceType.GetGenericArguments()[0];
                    var responseType = c.ServiceType.GetGenericArguments()[1];

                    var concreteType = container.GetRootRegistrations()
                        .FirstOrDefault(reg => reg.ServiceType.GetInterfaces().Any(i => i == c.ServiceType))?
                        .ServiceType;

                    if (concreteType == null)
                    {
                        throw new BusinessAppWebApiException(
                            $"No command handler is setup for command '{requestType.Name}'. Please set one up.");
                    }
                    else
                    {
                        return c.Consumer?.ImplementationType.GetGenericTypeDefinition() == typeof(BatchRequestDelegator<,>)
                            ? typeof(BatchScopeWrappingHandler<,,>).MakeGenericType(concreteType, requestType, responseType)
                            : concreteType;
                    }
                },
                Lifestyle.Scoped,
                c => !c.Handled);
        }

        private static bool HasTransactionScope(DecoratorPredicateContext ctx)
        {
            return !ctx.ImplementationType.IsConstructedGenericType ||
            (
                ctx.ImplementationType.GetGenericTypeDefinition() == typeof(BatchRequestDelegator<,>) ||
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
            BatchScopeWrappingHandler<BatchRequestDelegator<TRequest, TResponse>, IEnumerable<TRequest>, IEnumerable<TResponse>>
        {
            public MacroScopeWrappingHandler(BatchRequestDelegator<TRequest, TResponse> inner)
                : base(inner)
            {
            }
        }

        public class BatchScopeWrappingHandler<TConsumer, TRequest, TResponse> :
            IRequestHandler<TRequest, TResponse>
            where TConsumer : IRequestHandler<TRequest, TResponse>
        {
            private readonly TConsumer inner;

            public BatchScopeWrappingHandler(TConsumer inner)
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
