namespace BusinessApp.WebApi
{
    using SimpleInjector;
#if efcore
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using System;
//#if DEBUG
    using Microsoft.Extensions.Logging;
//#endif
    using BusinessApp.App;
#endif
    using System.Reflection;
    using BusinessApp.Data;
    using BusinessApp.Domain;

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
            GuardAgainst.Null(container, nameof(container));

            container.Register(typeof(IAggregateRootRepository<,>), Assembly);
            container.Register(typeof(IQueryVisitorFactory<,>), typeof(CompositeQueryVisitorBuilder<,>));
            container.Register(typeof(ILinqSpecificationBuilder<,>), typeof(AndSpecificationBuilder<,>));
            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>), Assembly);
            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
#if efcore
                typeof(EFQueryVisitorFactory<,>),
                typeof(EFQueryFieldsVisitorFactory<,>)
#endif
            });
#if efcore
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
            container.RegisterConditional(
                typeof(IQueryHandler<,>),
                typeof(EFSingleQueryStrategyHandler<,>),
                ctx => !ctx.Handled
            );
            container.Register<IEventRepository, EventRepository>();
            container.Register<IUnitOfWork>(() => container.GetInstance<BusinessAppDbContext>());
            container.Register<ITransactionFactory>(() => container.GetInstance<BusinessAppDbContext>());

            RegisterDbContext<BusinessAppDbContext>(container, options.WriteConnectionString);
            RegisterDbContext<BusinessAppReadOnlyDbContext>(container, options.ReadConnectionString);
#endif
        }

#if efcore
        public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppReadOnlyDbContext>
        {
            public BusinessAppReadOnlyDbContext CreateDbContext(string[] args)
            {
                var config = (IConfiguration)Program.CreateWebHostBuilder(new string[0])
                    .Build()
                    .Services
                    .GetService(typeof(IConfiguration));
                var connection = config.GetConnectionString("Main");
                var optionsBuilder = new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>();

                optionsBuilder.UseSqlServer(connection, x => x.MigrationsAssembly("BusinessApp.Data"));

                return new BusinessAppReadOnlyDbContext(
                    optionsBuilder.Options
                );
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
#endif
    }
}
