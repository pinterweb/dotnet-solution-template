using Microsoft.EntityFrameworkCore;
//#if DEBUG
using Microsoft.Extensions.Logging;
//#endif
using BusinessApp.Infrastructure.EntityFramework;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    public class EFCoreRegister : IBootstrapRegister
    {
//#if DEBUG
        public static readonly ILoggerFactory DataLayerLoggerFactory
            = LoggerFactory.Create(builder =>
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddConsole()
                    .AddDebug());
//#endif

        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public EFCoreRegister(RegistrationOptions options, IBootstrapRegister inner)
        {
            this.options = options;
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            var requestHandlerType = typeof(IRequestHandler<,>);
            var container = context.Container;

            static bool CanHandle(PredicateContext context)
            {
                return !context.Handled
                    && context.HasConsumer
                    && context.Consumer.ImplementationType != context.ImplementationType;
            }

            RegisterHandlers(container);

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(EFMetadataStoreRequestDecorator<,>),
                c => !c.ServiceType.GetGenericArguments()[0].IsGenericIEnumerable()
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && !c.ImplementationType.Name.Contains("Decorator")
                    && !typeof(ICompositeEvent).IsAssignableFrom(c.ServiceType.GetGenericArguments()[1]));

            inner.Register(context);

            context.Container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(EFTrackingQueryDecorator<,>),
                c => c.ImplementationType.IsTypeDefinition(typeof(AuthorizationRequestDecorator<,>)));

            container.Collection.Append(
                typeof(IQueryVisitorFactory<,>),
                typeof(EFQueryVisitorFactory<,>));

            container.Register<IEventStoreFactory, EFEventStoreFactory>();

            container.Register(typeof(IDbSetVisitorFactory<,>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IDbSetVisitorFactory<,>),
                typeof(NullDbSetVisitorFactory<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(EFEnvelopedQueryHandler<,>),
                ctx => CanHandle(ctx));

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(EFQueryStrategyHandler<,>),
                ctx => CanHandle(ctx)
            );

            container.Register<EFUnitOfWork>();
            container.Register<ITransactionFactory>(() => container.GetInstance<EFUnitOfWork>());

            container.Register<BusinessAppDbContext>();
            container.Register<IRequestStore>(() => container.GetInstance<BusinessAppDbContext>());
            container.RegisterInstance(
              new DbContextOptionsBuilder<BusinessAppDbContext>()
//#if DEBUG
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(DataLayerLoggerFactory)
//#endif
                    .UseSqlServer(options.DbConnectionString)
                    .Options
            );
        }

        private void RegisterHandlers(Container container)
        {
            container.Register(typeof(IQueryVisitorFactory<,>),
                typeof(CompositeQueryVisitorBuilder<,>));

            container.Register(typeof(ILinqSpecificationBuilder<,>),
                typeof(AndSpecificationBuilder<,>));

            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>),
                options.RegistrationAssemblies);

            container.Collection.Register(typeof(IQueryVisitor<>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IQueryVisitor<>),
                typeof(NullQueryVisitor<>), ctx => !ctx.Handled);

            container.Collection.Append(typeof(ILinqSpecificationBuilder<,>), typeof(QueryOperatorSpecificationBuilder<,>));

            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
                typeof(ConstructedQueryVisitorFactory<,>),
            });
        }
    }
}
