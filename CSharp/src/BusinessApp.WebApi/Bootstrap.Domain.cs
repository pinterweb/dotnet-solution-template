namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using System.Reflection;
    using BusinessApp.Domain;
    using System.Linq;

    /// <summary>
    /// Allows registering all types that are defined in the domain layer
    /// </summary>
    public static partial class Bootstrap
    {
        public static void Domain(Container container, BootstrapOptions options)
        {
            container.NotNull().Expect(nameof(container));

            container.Collection.Register(typeof(IEventHandler<>), options.RegistrationAssemblies);

            container.Collection.Append(typeof(IEventHandler<>), typeof(DomainEventHandler<>));
            container.Register<IUnitOfWork, EventUnitOfWork>();

            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);
        }
    }
}
