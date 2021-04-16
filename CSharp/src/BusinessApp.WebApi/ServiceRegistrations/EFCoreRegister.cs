namespace BusinessApp.WebApi
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    //#if DEBUG
    using Microsoft.Extensions.Logging;
    //#endif
    using BusinessApp.Data;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using SimpleInjector;
    using Microsoft.AspNetCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Hosting;

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

        private readonly BootstrapOptions options;
        private readonly IBootstrapRegister inner;

        public EFCoreRegister(BootstrapOptions options,
            IBootstrapRegister inner)
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
                    && !typeof(IEventStream).IsAssignableFrom(c.ServiceType.GetGenericArguments()[1]));

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

        public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppDbContext>
        {
            public BusinessAppDbContext CreateDbContext(string[] args)
            {
                var config = (IConfiguration?)WebHost.CreateDefaultBuilder(args)
                    .ConfigureServices(sc => sc.AddSingleton(new Container()))
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder.AddCommandLine(args);
                        builder.AddEnvironmentVariables();
                    })
                    .UseStartup<Startup>()
                    .Build()
                    .Services
                    .GetService(typeof(IConfiguration));

#if docker
                var connection = config.GetConnectionString("docker");
#else
                var connection = config.GetConnectionString("local");
#endif
                var optionsBuilder = new DbContextOptionsBuilder<BusinessAppDbContext>();

                optionsBuilder.UseSqlServer(connection, x => x.MigrationsAssembly("BusinessApp.Data"));

                return new BusinessAppDbContext(optionsBuilder.Options);
            }
        }
    }
}
