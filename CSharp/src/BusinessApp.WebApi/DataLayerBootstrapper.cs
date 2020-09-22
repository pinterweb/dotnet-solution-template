namespace BusinessApp.WebApi
{
    using SimpleInjector;
#if efcore
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
//#if DEBUG
    using Microsoft.Extensions.Logging;
//#endif
#endif
    using System.Reflection;
    using BusinessApp.Data;
    using BusinessApp.Domain;
    using BusinessApp.App;

    /// <summary>
    /// Allows registering all types that are defined in the data layer
    /// </summary>
    public static class DataLayerBootstrapper
    {
        public static readonly Assembly Assembly = typeof(IQueryVisitor<>).Assembly;
#if efcore
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
#endif

        public static void Bootstrap(Container container, BootstrapOptions options)
        {
            Guard.Against.Null(container).Expect(nameof(container));

            container.Register(typeof(IQueryVisitorFactory<,>), typeof(CompositeQueryVisitorBuilder<,>));
            container.Register(typeof(ILinqSpecificationBuilder<,>), typeof(AndSpecificationBuilder<,>));
            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>), Assembly);
            container.Collection.Register(typeof(IQueryVisitor<>), Assembly);
            container.RegisterConditional(
                typeof(IQueryVisitor<>),
                typeof(NullQueryVisitor<>), ctx => !ctx.Handled);
            container.Collection.Append(typeof(ILinqSpecificationBuilder<,>), typeof(QueryOperatorSpecificationBuilder<,>));
            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
                typeof(ConstructedQueryVisitorFactory<,>),
#if efcore
                typeof(EFQueryVisitorFactory<,>),
#endif
            });
#if efcore
            container.Register(typeof(IDatastore<>), typeof(EFDatastore<>));
            container.Register(typeof(IDbSetVisitorFactory<,>), Assembly);
            container.RegisterConditional(
                typeof(IDbSetVisitorFactory<,>),
                typeof(NullDbSetVisitorFactory<,>),
                ctx => !ctx.Handled);

            container.RegisterConditional(
                typeof(IQueryHandler<,>),
                typeof(EFQueryStrategyHandler<,>),
                ctx => !ctx.Handled
            );
            container.RegisterConditional(typeof(IQueryHandler<,>),
                typeof(EFEnvelopedQueryHandler<,>),
                ctx => !ctx.Handled);
            container.RegisterConditional(
                typeof(IQueryHandler<,>),
                typeof(EFSingleQueryStrategyHandler<,>),
                ctx => !ctx.Handled
            );
            container.Register<IEventRepository, EventRepository>();
            container.Register<EFUnitOfWork>();
            container.Register<IUnitOfWork>(() => container.GetInstance<EFUnitOfWork>());
            container.Register<ITransactionFactory>(() => container.GetInstance<EFUnitOfWork>());

            RegisterDbContext<BusinessAppDbContext>(container, options.WriteConnectionString);
#else
            container.Register<ITransactionFactory, NullTransactionFactory>();
#endif
        }

#if efcore

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


        private static void RegisterDbContext<TContext>(Container container, string connectionString)
            where TContext : DbContext
        {
            container.Register<TContext>();
            container.RegisterInstance(
              new DbContextOptionsBuilder<TContext>()
//#if DEBUG
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(DataLayerLoggerFactory)
//#endif
                    .UseSqlServer(connectionString)
                    .Options
            );
        }
#else
        // TODO - until i can implement Dapper as opposed to EF
        public sealed class NullTransactionFactory : ITransactionFactory
        {
            public IUnitOfWork Begin()
            {
                throw new System.NotImplementedException();
            }
        }
#endif
    }
}
