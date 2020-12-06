namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.Domain;
    using System.Linq;

    /// <summary>
    /// Allows registering all types that are defined in the domain layer
    /// </summary>
    public static class DomainLayerBoostrapper
    {
        private static readonly Assembly Assembly = typeof(IEventHandler<>).Assembly;

        public static void Bootstrap(Container container, BootstrapOptions options)
        {
            container.NotNull().Expect(nameof(container));

            container.Collection.Register(typeof(IEventHandler<>), new[]
            {
                Assembly,
                WebApiBootstrapper.Assembly
            }.Concat(options.AppAssemblies).Concat(options.DataAssemblies));

            container.Collection.Append(typeof(IEventHandler<>), typeof(DomainEventHandler<>));
            container.Register<IUnitOfWork, EventUnitOfWork>();

            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);
        }
    }
}
