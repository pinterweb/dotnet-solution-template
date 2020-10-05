namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.App;
    using SimpleInjector;

    public static partial class SimpleInjectorRegistrationExtensions
    {
        public static void RegisterQueryDecorator(this Container container,
            Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate = null)
        {
            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                decoratorType,
                ctx => (predicate == null ? true : predicate(ctx)) &&
                    ctx.ServiceType.GetGenericArguments()[0].IsQueryType());
        }

        public static void RegisterCommandDecorator(this Container container,
            Type decoratorType,
            Predicate<DecoratorPredicateContext> predicate = null,
            Lifestyle lifestyle = null)
        {
            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                decoratorType,
                lifestyle,
                ctx => (predicate == null ? true : predicate(ctx)) &&
                    !ctx.ServiceType.GetGenericArguments()[0].IsQueryType());
        }

        public static bool IsQueryType(this Type type)
        {
            return typeof(Query).IsAssignableFrom(type);
        }
    }
}
