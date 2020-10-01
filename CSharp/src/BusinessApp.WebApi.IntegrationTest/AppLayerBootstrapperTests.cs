namespace BusinessApp.WebApi.IntegrationTest
{
    using Xunit;
    using SimpleInjector;
    using Microsoft.AspNetCore.Hosting;
    using FakeItEasy;
    using System.Linq;
    using BusinessApp.App;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using BusinessApp.Domain;
    using System;

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
            }

            [Fact]
            public void NotABatchCommand_NoBatchDecoratorsInHandlers()
            {
                /* Arrange */
                container.Register(
                    typeof(ICommandHandler<CommandStub>),
                    typeof(CommandHandlerStub)
                );
                CreateRegistrations(container);

                /* Act */
                container.Verify();

                var firstType = container.GetRegistration(typeof(ICommandHandler<CommandStub>));
                var handlers = firstType
                    .GetDependencies()
                    .Where(i => typeof(ICommandHandler<CommandStub>).IsAssignableFrom(i.ServiceType))
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(ValidationCommandDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<CommandStub>),
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
                   typeof(ICommandHandler<CommandStub>),
                   typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub>)
               );
               CreateRegistrations(container);

                /* Act */
                container.Verify();
                var _ = container.GetInstance<ICommandHandler<IEnumerable<CommandStub>>>();

                var firstType = container.GetRegistration(typeof(ICommandHandler<IEnumerable<CommandStub>>));
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(ICommandHandler<IEnumerable<CommandStub>>).IsAssignableFrom(i.ServiceType) ||
                        typeof(ICommandHandler<CommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(ValidationCommandDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationBatchCommandDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandGroupDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ApplicationScopeBatchDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandHandler<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub>),
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
                   typeof(ICommandHandler<CommandStub>),
                   typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub>)
               );
               CreateRegistrations(container);

                /* Act */
                container.Verify();
                var _ = container.GetInstance<ICommandHandler<MacroStub>>();

                var firstType = container.GetRegistration(typeof(ICommandHandler<MacroStub>));
                var handlers = firstType
                    .GetDependencies()
                    .Where(i =>
                        typeof(ICommandHandler<MacroStub>).IsAssignableFrom(i.ServiceType) ||
                        typeof(ICommandHandler<IEnumerable<CommandStub>>).IsAssignableFrom(i.ServiceType) ||
                        typeof(ICommandHandler<CommandStub>).IsAssignableFrom(i.ServiceType)
                    )
                    .Prepend(firstType);

                /* Assert */
                Assert.Collection(handlers,
                    rel => Assert.Equal(
                        typeof(ValidationCommandDecorator<MacroStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchMacroCommandDecorator<MacroStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationCommandDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ValidationBatchCommandDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandGroupDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(ApplicationScopeBatchDecorator<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(DeadlockRetryDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(TransactionDecorator<IEnumerable<CommandStub>>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(BatchCommandHandler<CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(AppLayerBootstrapper.HandlerWrapper<CommandHandlerStub, CommandStub>),
                        rel.Registration.ImplementationType),
                    rel => Assert.Equal(
                        typeof(CommandHandlerStub),
                        rel.Registration.ImplementationType)
                );
            }
        }

        private sealed class CommandHandlerStub : ICommandHandler<CommandStub>
        {
            public Task<Result<CommandStub, IFormattable>> HandleAsync(CommandStub command, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        public sealed class CommandStub {}
        public sealed class MacroStub : IMacro<CommandStub> {}
    }
}
