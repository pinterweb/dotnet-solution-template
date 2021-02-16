namespace BusinessApp.WebApi.IntegrationTest
{
    using Xunit;
    using SimpleInjector;
    using Microsoft.AspNetCore.Hosting;
    using FakeItEasy;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Test;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System;
    using SimpleInjector.Lifestyles;
    using BusinessApp.Domain;
    using Microsoft.Extensions.Configuration;
    using BusinessApp.Data;
    using System.Linq.Expressions;

    public partial class BootstrapTests : IDisposable
    {
        private readonly Container container;
        private readonly Scope scope;

        public BootstrapTests()
        {
            container = new Container();
            new Startup(A.Dummy<IConfiguration>(), container);
            scope = AsyncScopedLifestyle.BeginScope(container);
        }

        public void Dispose() => scope.Dispose();

        public void CreateRegistrations(Container container, IWebHostEnvironment env = null)
        {
            var bootstrapOptions = new BootstrapOptions
            {
                DbConnectionString = "Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar",
                RegistrationAssemblies = new[]
                {
                    typeof(BootstrapTests).Assembly,
                    typeof(IQuery).Assembly,
                    typeof(IQueryVisitor<>).Assembly
                }
            };
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            Bootstrapper.RegisterServices(container,
                bootstrapOptions,
                (env ?? A.Dummy<IWebHostEnvironment>()));
        }

        public class Handlers : BootstrapTests, IDisposable
        {
            [Fact]
            public void NotABatchCommand_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<CommandStub, CommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(IRequestHandler<CommandStub, CommandStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void NonBatchAuthCommand_AuthDecoratorAddedWithoutBatchDecorators()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<AuthCommandStub, AuthCommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(IRequestHandler<AuthCommandStub, AuthCommandStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthCommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchCommand_BatchDecoratorsInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>)
                            .IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<CommandStub, CommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchAuthCommand_BatchDecoratorsWithAuthInHandlers()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(IRequestHandler<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>)
                            .IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<AuthCommandStub, AuthCommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<AuthCommandStub, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<AuthCommandStub>, IEnumerable<AuthCommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchScopeWrappingHandler<AuthCommandHandlerStub, AuthCommandStub, AuthCommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthCommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchMacroCommand_BatchMacroDecoratorsInHandlers()
            {
                /* Arrange */
                container.RegisterInstance(A.Fake<IBatchMacro<MacroStub, CommandStub>>());
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>).IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>)
                            .IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<CommandStub, CommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<MacroStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(MacroBatchRequestDelegator<MacroStub, CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(MacroScopeWrappingHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchMacroAuthCommand_BatchMacroWithAuthDecoratorsInHandlers()
            {
                /* Arrange */
                container.RegisterInstance(A.Fake<IBatchMacro<AuthMacroStub, CommandStub>>());
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<AuthMacroStub, IEnumerable<CommandStub>>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(IRequestHandler<AuthMacroStub, IEnumerable<CommandStub>>).IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>)
                            .IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<CommandStub, CommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<AuthMacroStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<AuthMacroStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<AuthMacroStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(MacroBatchRequestDelegator<AuthMacroStub, CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(MacroScopeWrappingHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void QueryRequest_QueryDecoratorsAdded()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<QueryStub, ResponseStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(IRequestHandler<QueryStub, ResponseStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EFTrackingQueryDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(QueryHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void QueryRequestWithAuth_AuthQueryDecoratorsAdded()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<AuthQueryStub, ResponseStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(IRequestHandler<AuthQueryStub, ResponseStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EFTrackingQueryDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthorizationRequestDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthQueryHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void NoExplicityQueryRequestRegistration_SingleQueryHandlerDecoratorsAdded()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IRequestHandler<UnregisteredQuery, UnregisteredQuery>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(IRequestHandler<UnregisteredQuery, UnregisteredQuery>).IsAssignableFrom(i.ServiceType) ||
                        typeof(IRequestHandler<UnregisteredQuery, IEnumerable<UnregisteredQuery>>).IsAssignableFrom(i.ServiceType)
                        )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EFTrackingQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(SingleQueryDelegator<EFQueryStrategyHandler<UnregisteredQuery, UnregisteredQuery>, UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EFQueryStrategyHandler<UnregisteredQuery, UnregisteredQuery>),
                        rel.Registration.ImplementationType)
                );
            }
        }

        public class Validators : BootstrapTests
        {
            [Fact]
            public void RegistersAllValidators()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IValidator<CommandStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType .GetDependencies() .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(CompositeValidator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(IEnumerable<IValidator<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DataAnnotationsValidator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(FluentValidationValidator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(IEnumerable<FluentValidation.IValidator<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(FirstFluentValidatorStub),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(SecondFluentValidatorStub),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(FirstValidatorStub),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(SecondValidatorStub),
                        rel.Registration.ImplementationType)
                );
            }

            private class FirstFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            {
            }

            private class SecondFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            {
            }

            private class FirstValidatorStub : IValidator<CommandStub>
            {
                public Task<Result> ValidateAsync(CommandStub instance, CancellationToken cancellationToken)
                {
                    return Task.FromResult(Result.Ok);
                }
            }

            private class SecondValidatorStub : IValidator<CommandStub>
            {
                public Task<Result> ValidateAsync(CommandStub instance, CancellationToken cancellationToken)
                {
                    return Task.FromResult(Result.Ok);
                }
            }
        }

        public class Loggers : BootstrapTests
        {
            [Fact]
            public void RegistersAllLoggersInDevMode()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Development");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(ILogger);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType.GetDependencies().Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(CompositeLogger),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(IEnumerable<ILogger>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TraceLogger),
                        rel.Registration.ImplementationType),
                    rel => Assert.IsType<BackgroundLogDecorator>(
                        Expression.Lambda(rel.BuildExpression()).Compile().DynamicInvoke()));
            }

            [Fact]
            public void RemovesTraceLoggerInOtherEnvironments()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(ILogger);

                /* Act */
                var _ = container.GetInstance(serviceType);

                var firstType = container.GetRegistration(serviceType);
                var handlers = firstType.GetDependencies().Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(CompositeLogger),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(IEnumerable<ILogger>),
                        rel.Registration.ImplementationType),
                    rel => Assert.IsType<BackgroundLogDecorator>(
                        Expression.Lambda(rel.BuildExpression()).Compile().DynamicInvoke()));
            }
        }

        private sealed class CommandHandlerStub : ICommandHandler<CommandStub>
        {
            public Task<Result> RunAsync(CommandStub request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AuthCommandHandlerStub : ICommandHandler<AuthCommandStub>
        {
            public Task<Result> RunAsync(AuthCommandStub request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class QueryHandlerStub : IQueryHandler<QueryStub, ResponseStub>
        {
            public Task<Result<ResponseStub, IFormattable>> HandleAsync(QueryStub request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class AuthQueryHandlerStub : IQueryHandler<AuthQueryStub, ResponseStub>
        {
            public Task<Result<ResponseStub, IFormattable>> HandleAsync(AuthQueryStub request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class CommandStub { }
        [Authorize]
        public sealed class AuthCommandStub { }
        [Authorize]
        public sealed class AuthQueryStub : QueryStub { }
        [Authorize]
        public sealed class AuthMacroStub : IMacro<CommandStub> { }
        public sealed class MacroStub : IMacro<CommandStub> { }
        public sealed class UnregisteredQuery : Query
        {
            public override IEnumerable<string> Sort { get; set; }
        }
    }
}
