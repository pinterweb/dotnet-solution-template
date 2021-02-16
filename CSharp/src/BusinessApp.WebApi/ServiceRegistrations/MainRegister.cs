namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.Domain;
    using BusinessApp.Data;

    public class MainRegister : IBootstrapRegister
    {
        private readonly BootstrapOptions options;

        public MainRegister(BootstrapOptions options)
        {
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;

            container.Register<IEventRepository, EventRepository>();
            container.Collection.Register(typeof(IEventHandler<>), options.RegistrationAssemblies);
            container.Collection.Append(typeof(IEventHandler<>), typeof(DomainEventHandler<>));

            container.Register<IUnitOfWork, EventUnitOfWork>();

            container.Register<PostCommitRegister>();
            container.Register<IPostCommitRegister>(container.GetInstance<PostCommitRegister>);
        }
    }
}
