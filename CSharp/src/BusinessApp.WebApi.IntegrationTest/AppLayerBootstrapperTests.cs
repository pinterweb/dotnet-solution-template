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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System;
    using SimpleInjector.Lifestyles;
    using BusinessApp.Domain;
    using Microsoft.Extensions.Configuration;

    public class AppLayerBootstrapperTests
    {
        public void CreateRegistrations(Container container)
        {
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            WebApiBootstrapper.Bootstrap(
                A.Dummy<IApplicationBuilder>(),
                A.Dummy<IWebHostEnvironment>(),
                container,
                A.Dummy<BootstrapOptions>());
        }

        public class Bootstrap : AppLayerBootstrapperTests, IDisposable
        {
            private readonly Container container;
            private readonly Scope scope;

            public Bootstrap()
            {
                container = new Container();
                new Startup(A.Dummy<IConfiguration>(), container);
                scope = AsyncScopedLifestyle.BeginScope(container);
            }

            [Fact]
            public void NotABatchCommand_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                container.Register<CommandHandlerStub>();
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
                container.Register<AuthCommandHandlerStub>();
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
                container.Register<CommandHandlerStub>();
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
                        typeof(AppLayerBootstrapper.BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
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
                container.Register<AuthCommandHandlerStub>();
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
                        typeof(AppLayerBootstrapper.BatchScopeWrappingHandler<AuthCommandHandlerStub, AuthCommandStub, AuthCommandStub>),
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
                container.Register<CommandHandlerStub>();
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
                        typeof(AppLayerBootstrapper.MacroScopeWrappingHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
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
                container.Register<CommandHandlerStub>();
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
                        typeof(AppLayerBootstrapper.MacroScopeWrappingHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchRequestDelegator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.BatchScopeWrappingHandler<CommandHandlerStub, CommandStub, CommandStub>),
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
                container.Register<QueryHandlerStub>();
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
                        typeof(ValidationRequestDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<QueryStub, ResponseStub>),
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
                container.Register<AuthQueryHandlerStub>();
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
                        typeof(AuthorizationRequestDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(EntityNotFoundQueryDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(InstanceCacheQueryDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AuthQueryHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            public void Dispose() => scope.Dispose();
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
    }
}
