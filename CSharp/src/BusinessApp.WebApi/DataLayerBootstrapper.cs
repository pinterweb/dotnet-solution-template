namespace BusinessApp.WebApi
{
    using SimpleInjector;
#if efcore
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using System;
#if DEBUG
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
#endif
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
#if DEBUG
        public static readonly LoggerFactory LoggerFactory
            = new LoggerFactory(new[]
        {
            new ConsoleLoggerProvider((category, level) => category == DbLoggerCategory.Database.Command.Name
                   && level == LogLevel.Information, true)
        });
#endif
#endif

        public static void Bootstrap(Container container)
        {
            GuardAgainst.Null(container, nameof(container));

            container.Register(typeof(IAggregateRootRepository<,>), Assembly);
            container.Register(typeof(IQueryVisitorFactory<,>), typeof(CompositeQueryVisitorBuilder<,>));
            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
#if efcore
                typeof(EFQueryVisitorFactory<,>),
                typeof(EFQueryFieldsVisitorFactory<,>)
#endif
            });
            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>), Assembly);
#if efcore
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
            RegisterDbContext<BusinessAppDbContext>(container);
            RegisterDbContext<BusinessAppReadOnlyDbContext>(container);
#endif
        }

#if efcore
        public sealed class MigrationsContextFactory : IDesignTimeDbContextFactory<BusinessAppReadOnlyDbContext>
        {
            public MigrationsContextFactory()
            {
                DotNetEnv.Env.Load("./.env");
            }

            public BusinessAppReadOnlyDbContext CreateDbContext(string[] args)
            {
                var connection = Environment.GetEnvironmentVariable("SQLCONNSTR_BUSINESSAPPLICATION");
                var optionsBuilder = new DbContextOptionsBuilder<BusinessAppReadOnlyDbContext>();

                optionsBuilder.UseSqlServer(connection, x => x.MigrationsAssembly("BusinessApp.Data"));

                return new BusinessAppReadOnlyDbContext(
                    optionsBuilder.Options
                );
            }
        }

        private static void RegisterDbContext<TContext>(Container container)
            where TContext : DbContext
        {
            container.Register<TContext>();
            container.RegisterInstance(
              new DbContextOptionsBuilder<TContext>()
#if DEBUG
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(LoggerFactory)
#endif
                    .UseSqlServer(Environment.GetEnvironmentVariable("SQLCONNSTR_BUSINESSAPPLICATION"))
                    .Options
            );
        }
#endif
    }
}
