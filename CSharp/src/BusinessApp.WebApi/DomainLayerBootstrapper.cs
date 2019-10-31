namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.Domain;

    /// <summary>
    /// Allows registering all types that are defined in the domain layer
    /// </summary>
    public static class DomainLayerBoostrapper
    {
        private static readonly Assembly Assembly = typeof(IEventHandler<>).Assembly;

        public static void Bootstrap(Container container)
        {
            GuardAgainst.Null(container, nameof(container));

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                Assembly,
                AppLayerBootstrapper.Assembly,
                DataLayerBootstrapper.Assembly,
                WebApiBootstrapper.Assembly
            });

            container.Collection.Append(typeof(IEventHandler<>), typeof(DomainEventHandler<>));
            container.Register<EventUnitOfWork>();
        }
    }
}
