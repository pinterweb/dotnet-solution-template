namespace BusinessApp.WebApi.IntegrationTest
{
    using Xunit;
    using SimpleInjector;
    using Microsoft.AspNetCore.Hosting;
    using FakeItEasy;
    using System.Linq;
    using BusinessApp.App;
    using BusinessApp.Test.Shared;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System;
    using SimpleInjector.Lifestyles;
    using BusinessApp.Domain;
    using Microsoft.Extensions.Configuration;
    using BusinessApp.Data;
    using Microsoft.Extensions.Logging;
    using BusinessApp.WebApi.Json;
    using Microsoft.Extensions.Localization;

    public class ServiceRegistrationsTests : IDisposable
    {
        private readonly Container container;
        private readonly Scope scope;

        public ServiceRegistrationsTests()
        {
            container = new Container();
            var config = A.Fake<IConfiguration>();
            var configSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => config.GetSection("ConnectionStrings")).Returns(configSection);
            A.CallTo(() => configSection["local"]).Returns("foo");
            new Startup(config, container);
            scope = AsyncScopedLifestyle.BeginScope(container);
        }

        public void Dispose() => scope.Dispose();

        public void CreateRegistrations(Container container, IWebHostEnvironment env = null)
        {
            var bootstrapOptions = new BootstrapOptions("Server=(localdb)\\MSSQLLocalDb;Initial Catalog=foobar")
            {
                RegistrationAssemblies = new[]
                {
                    typeof(ServiceRegistrationsTests).Assembly,
                    typeof(IQuery).Assembly,
                    typeof(IQueryVisitor<>).Assembly
                }
            };
            container.RegisterInstance(A.Fake<IHttpContextAccessor>());
            container.RegisterInstance(A.Fake<IStringLocalizerFactory>());
            container.RegisterInstance(A.Fake<IBatchMacro<MacroStub, CommandStub>>());
            container.RegisterInstance(A.Fake<IBatchMacro<NoHandlerMacroStub, NoHandlerCommandStub>>());
            Bootstrapper.RegisterServices(container,
                bootstrapOptions,
                (env ?? A.Dummy<IWebHostEnvironment>()),
                A.Dummy<ILoggerFactory>());
        }


        public class RequestHandlers : ServiceRegistrationsTests
        {
            public class CommandRequests : ServiceRegistrationsTests
            {
                IEnumerable<(Type, bool)> expectedServicesAndBatchOnlyFlag;

                public CommandRequests()
                {
                    expectedServicesAndBatchOnlyFlag = new[]
                    {
                        (typeof(RequestExceptionDecorator<,>), false),
                        (typeof(GroupedBatchRequestDecorator<,>), true),
                        (typeof(ScopedBatchRequestProxy<,>), true),
                        (typeof(AuthorizationRequestDecorator<,>), false),
                        (typeof(ValidationRequestDecorator<,>), false),
                    };
                }

                public static IEnumerable<object[]> HandlerTypes => new[]
                {
                    new object[] { typeof(IRequestHandler<NoHandlerCommandStub, NoHandlerCommandStub>) },
                    new object[] { typeof(IRequestHandler<CommandStub, EventStreamStub>) },
                    new object[] { typeof(IRequestHandler<CommandStub, CommandStub>) },
                    new object[] { typeof(IRequestHandler<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>) },
                    new object[] { typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>) },
                    new object[] { typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>) },
                    new object[] { typeof(IRequestHandler<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>) },
                    new object[] { typeof(IRequestHandler<MacroStub, IEnumerable<EventStreamStub>>) },
                    new object[] { typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>) },
                };

                [Fact]
                public void IsDummy()
                {
                    /* Assert */
                    Assert.Null(typeof(PostOrPut.Body).Assembly.FullName);
                }

                [Theory, MemberData(nameof(HandlerTypes))]
                public void NoConsumer_HasExceptionAuthAndValidationAsFirstThree(Type serviceType)
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var isBatchService = serviceType.GetGenericArguments()[0].IsGenericIEnumerable();
                    var take = isBatchService ? 5 : 3;

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(serviceType);

                    Assert.Equal(
                        expectedServicesAndBatchOnlyFlag
                            .Where(t => t.Item2 == isBatchService || !t.Item2)
                            .Select(i => i.Item1).ToList(),
                        handlers.Take(take).Select(i => i.GetGenericTypeDefinition()).ToList()
                    );
                }

                [Theory, MemberData(nameof(HandlerTypes))]
                public void HasDifferentConsumer_HasExceptionAuthAndValidationAsFirstThree(Type serviceType)
                {
                    /* Arrange */
                    var hateoasType = typeof(HateoasLink<,>).MakeGenericType(
                        serviceType.GetGenericArguments()[0],
                        typeof(IDomainEvent));
                    var hateoasImplType = typeof(Dictionary<,>).MakeGenericType(typeof(Type), hateoasType);
                    var hateoasSvcType = typeof(IDictionary<,>).MakeGenericType(typeof(Type), hateoasType);
                    Type MakeSvcGenericType(Type type)
                    {
                        return type.MakeGenericType(serviceType.GetGenericArguments()[0],
                           serviceType.GetGenericArguments()[1]);
                    }
                    container.Collection.Register(MakeSvcGenericType(typeof(HateoasLink<,>))
                        , new Type[0]);
                    container.RegisterInstance(hateoasSvcType, Activator.CreateInstance(hateoasImplType));
                    CreateRegistrations(container);
                    container.Verify();
                    container.GetInstance(MakeSvcGenericType(typeof(IHttpRequestHandler<,>)));
                    container.GetInstance(serviceType);
                    var isBatchService = serviceType.GetGenericArguments()[0].IsGenericIEnumerable();
                    var take = isBatchService ? 5 : 3;

                    /* Act */
                    var firstType = container.GetRegistration(MakeSvcGenericType(typeof(IHttpRequestHandler<,>)));

                    /* Assert */
                    var graph = firstType
                        .GetDependencies()
                        .Where(t => t.ServiceType == serviceType)
                        .Select(ip => ip.Registration.ImplementationType);
                    Assert.Equal(
                        expectedServicesAndBatchOnlyFlag
                            .Where(t => t.Item2 == isBatchService || !t.Item2)
                            .Select(i => i.Item1).ToList(),
                        graph.Take(take).Select(i => i.GetGenericTypeDefinition()).ToList()
                    );
                }
            }

            public class SingleRequest : ServiceRegistrationsTests
            {
                [Fact]
                public void NoHandler_UsesNoBusinessLogicHandler()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<NoHandlerCommandStub, NoHandlerCommandStub>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(serviceType);

                    Assert.Collection(handlers,
                          implType => Assert.Equal(
                              typeof(RequestExceptionDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(AuthorizationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(ValidationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(DeadlockRetryRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(TransactionRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(EFMetadataStoreRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                              implType),
                          implType => Assert.Equal(
                              typeof(NoBusinessLogicRequestHandler<NoHandlerCommandStub>),
                              implType)
                      );
                }

                [Fact]
                public void WithEventStreamResponse_HasSingleAndEventDecorators()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<CommandStub, EventStreamStub>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(serviceType);

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AutomationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStreamStub),
                            implType)
                    );
                }

                [Fact]
                public void WithoutEventStreamResponse_HasSingleRequestDecorators()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<CommandStub, CommandStub>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(serviceType);

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStub),
                            implType)
                    );
                }
            }

            public class BatchRequest : ServiceRegistrationsTests
            {
                [Fact]
                public void NoHandler_UsesNoBusinessLogicHandler()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                        typeof(IRequestHandler<NoHandlerCommandStub, NoHandlerCommandStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<NoHandlerCommandStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(NoBusinessLogicRequestHandler<NoHandlerCommandStub>),
                            implType)
                    );
                }

                [Fact]
                public void WithEventStreamResponse_HasBatchAndEventDecorators()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        typeof(IRequestHandler<CommandStub, EventStreamStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AutomationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStreamStub),
                            implType)
                    );
                }

                [Fact]
                public void WithoutEventStreamResponse_HasBatchDecorators()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        typeof(IRequestHandler<CommandStub, CommandStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStub),
                            implType)
                    );
                }
            }

            public class MacroRequest : ServiceRegistrationsTests
            {
                [Fact]
                public void NoHandler_UsesNoBusinessLogicHandler()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>),
                        typeof(IRequestHandler<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                        typeof(IRequestHandler<NoHandlerCommandStub, NoHandlerCommandStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<NoHandlerMacroStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(MacroBatchRequestAdapter<NoHandlerMacroStub, NoHandlerCommandStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<NoHandlerCommandStub, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<NoHandlerCommandStub>, IEnumerable<NoHandlerCommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<NoHandlerCommandStub, NoHandlerCommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(NoBusinessLogicRequestHandler<NoHandlerCommandStub>),
                            implType)
                    );
                }

                [Fact]
                public void WithEventStreamResponse_BatchMacroDecoratorsInHandlers()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<MacroStub, IEnumerable<EventStreamStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<MacroStub, IEnumerable<EventStreamStub>>),
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                        typeof(IRequestHandler<CommandStub, EventStreamStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<MacroStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(MacroBatchRequestAdapter<MacroStub, CommandStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<EventStreamStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AutomationRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EventConsumingRequestDecorator<CommandStub, EventStreamStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStreamStub),
                            implType)
                    );
                }

                [Fact]
                public void WithOutStreamResponse_BatchMacroDecoratorsInHandlers()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<MacroStub, IEnumerable<CommandStub>>),
                        typeof(IRequestHandler<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                        typeof(IRequestHandler<CommandStub, CommandStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<MacroStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<MacroStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(MacroBatchRequestAdapter<MacroStub, CommandStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(GroupedBatchRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ScopedBatchRequestProxy<CommandStub, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(TransactionRequestDecorator<IEnumerable<CommandStub>, IEnumerable<CommandStub>>),
                            implType),
                        implType => Assert.Equal(
                            typeof(BatchRequestAdapter<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFMetadataStoreRequestDecorator<CommandStub, CommandStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(CommandHandlerStub),
                            implType)
                    );
                }
            }

            public class QueryRequest : ServiceRegistrationsTests
            {
                [Fact]
                public void QueryRequest_QueryDecoratorsAdded()
                {
                    /* Arrange */
                    CreateRegistrations(container);
                    container.Verify();
                    var serviceType = typeof(IRequestHandler<QueryStub, ResponseStub>);

                    /* Act */
                    var _ = container.GetInstance(serviceType);

                    /* Assert */
                    var handlers = GetServiceGraph(typeof(IRequestHandler<QueryStub, ResponseStub>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFTrackingQueryDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(InstanceCacheQueryDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EntityNotFoundQueryDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<QueryStub, ResponseStub>),
                            implType),
                        implType => Assert.Equal(
                            typeof(QueryHandlerStub),
                            implType)
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

                    /* Assert */
                    var handlers = GetServiceGraph(
                        typeof(IRequestHandler<UnregisteredQuery, UnregisteredQuery>),
                        typeof(IRequestHandler<UnregisteredQuery, IEnumerable<UnregisteredQuery>>));

                    Assert.Collection(handlers,
                        implType => Assert.Equal(
                            typeof(RequestExceptionDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFTrackingQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(InstanceCacheQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EntityNotFoundQueryDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(AuthorizationRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(ValidationRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(DeadlockRetryRequestDecorator<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(SingleQueryRequestAdapter<UnregisteredQuery, UnregisteredQuery>),
                            implType),
                        implType => Assert.Equal(
                            typeof(EFQueryStrategyHandler<UnregisteredQuery, UnregisteredQuery>),
                            implType)
                    );
                }
            }
        }

        public class HttpRequestHandlers : ServiceRegistrationsTests
        {
            [Fact]
            public void HasCorrectOrder()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IHttpRequestHandler);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(HttpRequestLoggingDecorator),
                        implType),
                    implType => Assert.Equal(
                        typeof(HttpRequestBodyAnalyzer),
                        implType),
                    implType => Assert.Equal(
                        typeof(SimpleInjectorHttpRequestHandler),
                        implType)
                );
            }

            [Fact]
            public void OfTR_HasCorrectOrder()
            {
                /* Arrange */
                var linkFactory = A.Fake<Func<CommandStub, EventStreamStub, string>>();
                container.Collection.Register<HateoasLink<CommandStub, EventStreamStub>>(new[]
                {
                    new HateoasLink<CommandStub, EventStreamStub>(linkFactory, "foo")
                });
                container.RegisterInstance<IDictionary<Type, HateoasLink<CommandStub, IDomainEvent>>>(
                    new Dictionary<Type, HateoasLink<CommandStub, IDomainEvent>>());
                CreateRegistrations(container);
                container.Verify();
                var serviceType = typeof(IHttpRequestHandler<CommandStub, EventStreamStub>);

                /* Act */
                var _ = container.GetInstance(serviceType);

                /* Assert */
                var handlers = GetServiceGraph(serviceType);

                Assert.Collection(handlers,
                    implType => Assert.Equal(
                        typeof(HttpResponseDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(HttpRequestLoggingDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(WeblinkingHeaderRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(WeblinkingHeaderEventRequestDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(JsonHttpDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(NewtonsoftJsonExceptionDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(SystemJsonExceptionDecorator<CommandStub, EventStreamStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(HttpRequestHandler<CommandStub, EventStreamStub>),
                        implType)
                );
            }
        }

        public class Validators : ServiceRegistrationsTests
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

                /* Assert */
                var firstType = container.GetRegistration(serviceType);
                var validators = firstType
                    .GetDependencies()
                    .Prepend(firstType)
                    .Select(ip => ip.Registration.ImplementationType);

                Assert.Collection(validators,
                    implType => Assert.Equal(
                        typeof(CompositeValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(IEnumerable<IValidator<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(DataAnnotationsValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(FirstValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(SecondValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(FluentValidationValidator<CommandStub>),
                        implType),
                    implType => Assert.Equal(
                        typeof(IEnumerable<FluentValidation.IValidator<CommandStub>>),
                        implType),
                    implType => Assert.Equal(
                        typeof(FirstFluentValidatorStub),
                        implType),
                    implType => Assert.Equal(
                        typeof(SecondFluentValidatorStub),
                        implType)
                );
            }

            private class FirstFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            { }

            private class SecondFluentValidatorStub : FluentValidation.AbstractValidator<CommandStub>
            { }

            private class FirstValidatorStub : IValidator<CommandStub>
            {
                public Task<Result<Unit, Exception>> ValidateAsync(CommandStub instance, CancellationToken cancelToken)
                {
                    return Task.FromResult(Result.OK);
                }
            }

            private class SecondValidatorStub : IValidator<CommandStub>
            {
                public Task<Result<Unit, Exception>> ValidateAsync(CommandStub instance, CancellationToken cancelToken)
                {
                    return Task.FromResult(Result.OK);
                }
            }
        }

        public class Loggers : ServiceRegistrationsTests
        {
            [Fact]
            public void UsesStringLogEntryFormatterInDevMode()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Development");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(ILogEntryFormatter);

                /* Act */
                var instance = container.GetInstance(serviceType);

                /* Assert */
                Assert.IsType<StringLogEntryFormatter>(instance);
            }

            [Fact]
            public void UsesSerializedLogEntryFormatterNotInDevMode()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Production");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(ILogEntryFormatter);

                /* Act */
                var instance = container.GetInstance(serviceType);

                /* Assert */
                Assert.IsType<SerializedLogEntryFormatter>(instance);
            }
        }

        public class StringLocalization : ServiceRegistrationsTests
        {
            [Fact]
            public void HasConsumer_UsesConsumerAsGeneric()
            {
                /* Arrange */
                var env = A.Fake<IWebHostEnvironment>();
                A.CallTo(() => env.EnvironmentName).Returns("Development");
                CreateRegistrations(container, env);
                container.Verify();
                var serviceType = typeof(IRequestHandler<LocalizedCommand, LocalizedCommand>);

                /* Act */
                var instance = container.GetInstance(serviceType);

                /* Assert */
                var firstType = container.GetRegistration(serviceType);
                var graph = firstType
                    .GetDependencies()
                    .Select(ip => ip.Registration.ImplementationType);
                Assert.Contains(typeof(StringLocalizer<LocalizationConsumer>), graph);
            }

            [Fact]
            public void NoConsumer_UsesUnitAsGeneric()
            {
                /* Arrange */
                CreateRegistrations(container);
                container.Verify();

                /* Act */
                var instance = container.GetInstance(typeof(IStringLocalizer));

                /* Assert */
                Assert.IsType<StringLocalizer<Unit>>(instance);
            }

            private sealed class LocalizationConsumer : IRequestHandler<LocalizedCommand, LocalizedCommand>
            {
                private readonly IStringLocalizer localizer;

                public LocalizationConsumer(IStringLocalizer localizer)
                {
                    this.localizer = localizer;
                }

                public Task<Result<LocalizedCommand, Exception>> HandleAsync(LocalizedCommand request, CancellationToken cancelToken)
                {
                    throw new NotImplementedException();
                }
            }

            private sealed class LocalizedCommand {  }
        }

        private IEnumerable<Type> GetServiceGraph(params Type[] serviceTypes)
        {
            var firstType = container.GetRegistration(serviceTypes.First());

            return firstType
                .GetDependencies()
                .Where(i => serviceTypes.Any(st => st.IsAssignableFrom(i.ServiceType)))
                .Prepend(firstType)
                .Select(ip => ip.Registration.ImplementationType)
                .Where(t => t.IsVisible);
        }

        public sealed class CommandHandlerStreamStub : IRequestHandler<CommandStub, EventStreamStub>
        {
            public Task<Result<EventStreamStub, Exception>> HandleAsync(CommandStub request,
                CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class CommandHandlerStub : IRequestHandler<CommandStub, CommandStub>
        {
            public Task<Result<CommandStub, Exception>> HandleAsync(CommandStub request,
                CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class QueryHandlerStub : IQueryHandler<QueryStub, ResponseStub>
        {
            public Task<Result<ResponseStub, Exception>> HandleAsync(QueryStub request, CancellationToken cancelToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class NoHandlerCommandStub { }

        public sealed class CommandStub { }

        public sealed class EventStreamStub : IEventStream
        {
            public IEnumerable<IDomainEvent> Events { get; set; }
        }

        public sealed class MacroStub : IMacro<CommandStub> { }

        public sealed class NoHandlerMacroStub : IMacro<NoHandlerCommandStub> { }

        public sealed class UnregisteredQuery : Query
        {
            public override IEnumerable<string> Sort { get; set; }
        }
    }
}
