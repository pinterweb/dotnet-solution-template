using Microsoft.EntityFrameworkCore;
//#if DEBUG
using Microsoft.Extensions.Logging;
//#endif
using BusinessApp.Infrastructure.EntityFramework;
using BusinessApp.Infrastructure;
using BusinessApp.Domain;
using SimpleInjector;

namespace BusinessApp.CompositionRoot
{
    public class EFCoreRegister : IBootstrapRegister
    {
//#if DEBUG
        public static readonly ILoggerFactory DataLayerLoggerFactory
            = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter((category, level) =>
                        category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information)
                    .AddConsole()
                    .AddDebug();
            });
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

            container.RegisterDecorator(
                typeof(IRequestHandler<,>),
                typeof(EFMetadataStoreRequestDecorator<,>),
                c => !c.ServiceType.GetGenericArguments()[0].IsGenericIEnumerable()
                    && !c.ServiceType.GetGenericArguments()[0].IsQueryType()
                    && !c.ImplementationType.Name.Contains("Decorator")
                    && !typeof(ICompositeEvent).IsAssignableFrom(c.ServiceType.GetGenericArguments()[1]));

            inner.Register(context);

            container.Collection.Append(
                typeof(IQueryVisitorFactory<,>),
                typeof(EFQueryVisitorFactory<,>));

            container.Register<IEventStoreFactory, EFEventStoreFactory>();

            container.Register(typeof(IDbSetVisitorFactory<,>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IDbSetVisitorFactory<,>),
                typeof(NullDbSetVisitorFactory<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(EFQueryStrategyHandler<,>),
                ctx => CanHandle(ctx)
            );
            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(EFEnvelopedQueryHandler<,>),
                ctx => !ctx.Handled);

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
    }
}
