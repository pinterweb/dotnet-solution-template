namespace BusinessApp.WebApi
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BusinessApp.App;
    using SimpleInjector;

    public class RequestPipelineRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public RequestPipelineRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var serviceType = typeof(IRequestHandler<,>);
            var pipeline = context.GetPipeline(serviceType);

            // Request / Command Pipeline
            pipeline.RunOnce(typeof(RequestExceptionDecorator<,>))
                .RunOnce(typeof(AuthorizationRequestDecorator<,>))
                // Query  only
                .RunOnce(typeof(InstanceCacheQueryDecorator<,>))
                .Run(typeof(ValidationRequestDecorator<,>))
                // Query only
                .RunOnce(typeof(EntityNotFoundQueryDecorator<,>))
                .Run(typeof(GroupedBatchRequestDecorator<,>))
                .Run(typeof(ScopedBatchRequestProxy<,>))
                .RunOnce(typeof(DeadlockRetryRequestDecorator<,>), RequestType.Command)
                .RunOnce(typeof(TransactionRequestDecorator<,>), RequestType.Command);

            inner.Register(context);

            foreach (var d in pipeline.Build())
            {
                RegisterDecorator(d, serviceType, context.Container);
            }
        }

        private static void RegisterDecorator(
            (Type, PipelineOptions) context,
            Type serviceType,
            Container container)
        {
            Predicate<DecoratorPredicateContext> filter = context.Item2.ScopeBehavior switch
            {
                ScopeBehavior.Inner => (ctx) => IsInnerScope(ctx),
                ScopeBehavior.Outer => (ctx) => IsOuterScope(ctx),
                _ => (ctx) => true
            };

            var filter2 = context.Item2.RequestType switch
            {
                RequestType.Command => (ctx) => filter(ctx) &&
                    !ctx.ServiceType.GetGenericArguments()[0].IsQueryType(),
                RequestType.Query => (ctx) => filter(ctx) &&
                    ctx.ServiceType.GetGenericArguments()[0].IsQueryType(),
                _ => filter,
            };

            var filter3 = context.Item2.Test switch
            {
                null => filter2,
                _ => (ctx) => filter2(ctx) && context.Item2.Test(ctx.ServiceType, ctx.ImplementationType)
            };

            container.RegisterDecorator(
                serviceType,
                context.Item1,
                context.Item2.Lifetime.MapLifestyle(),
                filter3);
        }

        public static bool IsQueryType(Type type)
        {
            return typeof(IQuery).IsAssignableFrom(type);
        }

        public static bool IsOuterScope(DecoratorPredicateContext ctx)
        {
            var implType = ctx.ImplementationType;

            return !implType.IsConstructedGenericType ||
            (
                implType.GetGenericTypeDefinition() != typeof(BatchScopeWrappingHandler<,,>)
                && implType.GetGenericTypeDefinition() != typeof(MacroScopeWrappingHandler<,>)
            );
        }

        public static bool IsInnerScope(DecoratorPredicateContext ctx)
        {
            var implType = ctx.ImplementationType;

            return !implType.IsConstructedGenericType ||
            (
                implType.GetGenericTypeDefinition() == typeof(BatchRequestDelegator<,>)
                || implType.GetGenericTypeDefinition() == typeof(MacroScopeWrappingHandler<,>)
            );
        }
    }
}
