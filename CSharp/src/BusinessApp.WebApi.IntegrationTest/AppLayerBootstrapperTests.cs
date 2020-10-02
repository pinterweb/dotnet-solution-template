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

        public class Bootstrap : AppLayerBootstrapperTests
        {
            private readonly Container container;

            public Bootstrap()
            {
                container = new Container();
                container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            }

            [Fact]
            public void NotABatchCommand_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                container.Register(
                    typeof(IRequestHandler<CommandStub, CommandStub>),
                    typeof(CommandHandlerStub)
                );
                CreateRegistrations(container);

                /* Act */
                container.Verify();

                var firstType = container.GetRegistration(typeof(IRequestHandler<CommandStub, CommandStub>));
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(IRequestHandler<CommandStub, CommandStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(RequestExceptionDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchCommand_BatchDecoratorsInHandlers()
            {
                /* Arrange */
                container.Register(
                   typeof(IRequestHandler<CommandStub, CommandStub>),
                   typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub, CommandStub>)
               );
               CreateRegistrations(container);

                /* Act */
                container.Verify();
                var _ = container.GetInstance<IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>>();

                var firstType = container.GetRegistration(
                    typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>));
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
                        typeof(ValidationBatchCommandDecorator<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandGroupDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ApplicationScopeBatchDecorator<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void IsABatchMacroCommand_BatchMacroDecoratorsInHandlers()
            {
                /* Arrange */
                container.RegisterInstance(A.Fake<IBatchMacro<MacroStub, CommandStub>>());
                container.Register(
                   typeof(IRequestHandler<CommandStub, CommandStub>),
                   typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub, CommandStub>)
               );
               CreateRegistrations(container);

                /* Act */
                container.Verify();
                var _ = container.GetInstance<IRequestHandler<MacroStub, IEnumerable<CommandStub>>>();

                var firstType = container.GetRegistration(typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>));
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
                        typeof(BatchMacroCommandDecorator<MacroStub, CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationBatchCommandDecorator<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandGroupDecorator<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ApplicationScopeBatchDecorator<CommandStub, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandHandler<CommandStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub, CommandStub>),
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
                container.Register(
                    typeof(IRequestHandler<QueryStub, ResponseStub>),
                    typeof(CommandHandlerStub)
                );
                CreateRegistrations(container);

                /* Act */
                container.Verify();

                var firstType = container.GetRegistration(typeof(IRequestHandler<QueryStub, ResponseStub>));
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
                        typeof(QueryLifetimeCacheDecorator<QueryStub, ResponseStub>),
                        rel.Registration.ImplementationType)
                );
            }

            [Fact]
            public void QueryRequestWithAuth_AuthQueryDecoratorsAdded()
            {
                /* Arrange */
                container.Register(
                    typeof(IRequestHandler<AuthQueryStub, ResponseStub>),
                    typeof(CommandHandlerStub)
                );
                CreateRegistrations(container);

                /* Act */
                container.Verify();

                var firstType = container.GetRegistration(typeof(IRequestHandler<AuthQueryStub, ResponseStub>));
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
                        typeof(QueryLifetimeCacheDecorator<AuthQueryStub, ResponseStub>),
                        rel.Registration.ImplementationType)
                );
            }
        }

        private sealed class CommandHandlerStub : ICommandHandler<CommandStub>
        {
            public Task RunAsync(CommandStub request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class CommandStub {}
        public sealed class AuthQueryStub : QueryStub {}
        public sealed class MacroStub : IMacro<CommandStub> {}
    }
}
