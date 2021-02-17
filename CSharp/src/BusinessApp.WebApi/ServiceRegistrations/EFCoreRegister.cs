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
            var serviceType = typeof(IRequestHandler<,>);
            var pipeline = context.GetPipelineBuilder(serviceType);
            var container = context.Container;

            pipeline
                .IntegrateOnce(typeof(EFTrackingQueryDecorator<,>))
                .After(typeof(RequestExceptionDecorator<,>));

            inner.Register(context);

            container.Collection.Append(
                typeof(IQueryVisitorFactory<,>),
                typeof(EFQueryVisitorFactory<,>));

            container.Register(typeof(IDatastore<>), typeof(EFDatastore<>));

            container.Register(typeof(IDbSetVisitorFactory<,>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IDbSetVisitorFactory<,>),
                typeof(NullDbSetVisitorFactory<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IRequestHandler<,>),
                typeof(EFQueryStrategyHandler<,>),
                ctx => !ctx.Handled
            );
            container.RegisterConditional(typeof(IRequestHandler<,>),
                typeof(EFEnvelopedQueryHandler<,>),
                ctx => !ctx.Handled);

            container.Register<EFUnitOfWork>();
            container.Register<ITransactionFactory>(() => container.GetInstance<EFUnitOfWork>());

            container.Register<BusinessAppDbContext>();
            container.RegisterInstance(
              new DbContextOptionsBuilder<BusinessAppDbContext>()
//#if DEBUG
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(DataLayerLoggerFactory)
//#endif
                    .UseSqlServer(options.DbConnectionString)
                    .Options
            );

            container.Register<IDatabase>(() => container.GetInstance<BusinessAppDbContext>());
        }

        public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppDbContext>
        {
            public BusinessAppDbContext CreateDbContext(string[] args)
            {
                var config = (IConfiguration)Program.CreateWebHostBuilder(new string[0])
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
