using FakeItEasy;
using System.Collections.Generic;
using Xunit;
using BusinessApp.Kernel;
using BusinessApp.Test.Shared;
using System.Security.Principal;
using BusinessApp.Infrastructure;

namespace BusinessApp.Infrastructure.Persistence.IntegrationTest
{
    [Collection(nameof(DatabaseCollection))]
    public class EFEventStoreFactoryTests
    {
        private readonly EFEventStoreFactory sut;
        private readonly IEntityIdFactory<MetadataId> idFactory;
        private readonly IPrincipal user;
        private readonly BusinessAppDbContext db;

        public EFEventStoreFactoryTests(DbDatabaseFixture fixture)
        {
            idFactory = A.Fake<IEntityIdFactory<MetadataId>>();
            db = A.Fake<BusinessAppDbContext>();
            user = A.Fake<IPrincipal>();

            sut = new EFEventStoreFactory(db, idFactory, user);

            A.CallTo(() => user.Identity.Name).Returns("foo");
            A.CallTo(() => db.ChangeTracker).Returns(fixture.DbContext.ChangeTracker);
        }

        public class Constructor : EFEventStoreFactoryTests
        {
            public Constructor(DbDatabaseFixture fixture) : base(fixture)
            { }

            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<IEntityIdFactory<MetadataId>>(),
                    A.Dummy<IPrincipal>(),
                },
                new object[]
                {
                    A.Dummy<BusinessAppDbContext>(),
                    null,
                    A.Dummy<IPrincipal>(),
                },
                new object[]
                {
                    A.Dummy<BusinessAppDbContext>(),
                    A.Dummy<IEntityIdFactory<MetadataId>>(),
                    null,
                },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(BusinessAppDbContext d,
                IEntityIdFactory<MetadataId> f, IPrincipal p)
            {
                /* Arrange */
                void shouldThrow() => new EFEventStoreFactory(d, f, p);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }

        public class Create : EFEventStoreFactoryTests
        {
            public Create(DbDatabaseFixture fixture) : base(fixture)
            { }

            public class WhenMetadataExists : EFEventStoreFactoryTests
            {
                private readonly RequestStub trigger;

                public WhenMetadataExists(DbDatabaseFixture fixture) : base(fixture)
                {
                    trigger = new RequestStub();
                }

                [Fact]
                public void NotAddedToDb()
                {
                    /* Arrange */
                    var metadata = new Metadata<RequestStub>(A.Dummy<MetadataId>(),
                        "user", MetadataType.Request, trigger);
                    db.ChangeTracker.Context.Add(metadata);

                    /* Act */
                    _ = sut.Create(trigger);

                    /* Assert */
                    A.CallTo(() => db.Add(A<Metadata<RequestStub>>._)).MustNotHaveHappened();
                }

                [Fact]
                public void CorrlelationIdIsMetadataId()
                {
                    /* Arrange */
                    var id = new MetadataId(1);
                    var metadata = new Metadata<RequestStub>(id, "user", MetadataType.Request,
                        trigger);
                    db.ChangeTracker.Context.Add(metadata);
                    var store = sut.Create(trigger);

                    /* Act */
                    var trackingId = store.Add(A.Dummy<IEvent>());

                    /* Assert */
                    Assert.Same(id, trackingId.CorrelationId);
                }
            }

            public class WhenMetadataDoesNotExist : EFEventStoreFactoryTests
            {
                public WhenMetadataDoesNotExist(DbDatabaseFixture fixture) : base(fixture)
                { }

                [Fact]
                public void SetsMetadataId()
                {
                    /* Arrange */
                    var id = A.Dummy<MetadataId>();
                    Metadata<object> metadata = null;
                    A.CallTo(() => idFactory.Create()).Returns(id);
                    A.CallTo(() => db.Add(A<Metadata<object>>._))
                        .Invokes(c => metadata = c.GetArgument<Metadata<object>>(0));

                    /* Act */
                    var _ = sut.Create(A.Dummy<object>());

                    /* Assert */
                    Assert.Equal(id, metadata.Id);
                }

                [Theory]
                [InlineData("foouser", "foouser")]
                [InlineData(null, "Anonymous")]
                public void SetsMetadataUsername(string identityName, string setName)
                {
                    /* Arrange */
                    Metadata<object> metadata = null;
                    A.CallTo(() => user.Identity.Name).Returns(identityName);
                    A.CallTo(() => db.Add(A<Metadata<object>>._))
                        .Invokes(c => metadata = c.GetArgument<Metadata<object>>(0));

                    /* Act */
                    _ = sut.Create(A.Dummy<object>());

                    /* Assert */
                    Assert.Equal(setName, metadata.Username);
                }

                [Fact]
                public void NullIdentity_SetsAnonymousUsername()
                {
                    /* Arrange */
                    Metadata<object> metadata = null;
                    A.CallTo(() => user.Identity).Returns(null);
                    A.CallTo(() => db.Add(A<Metadata<object>>._))
                        .Invokes(c => metadata = c.GetArgument<Metadata<object>>(0));

                    /* Act */
                    _ = sut.Create(A.Dummy<object>());

                    /* Assert */
                    Assert.Equal(AnonymousUser.Name, metadata.Username);
                }

                [Fact]
                public void SetsMetadataEventTriggerType()
                {
                    /* Arrange */
                    Metadata<object> metadata = null;
                    A.CallTo(() => db.Add(A<Metadata<object>>._))
                        .Invokes(c => metadata = c.GetArgument<Metadata<object>>(0));

                    /* Act */
                    _ = sut.Create(A.Dummy<object>());

                    /* Assert */
                    Assert.Equal(MetadataType.EventTrigger.ToString(), metadata.TypeName);
                }

                [Fact]
                public void SetsMetadataTrigger()
                {
                    /* Arrange */
                    var trigger = A.Dummy<object>();
                    Metadata<object> metadata = null;
                    A.CallTo(() => db.Add(A<Metadata<object>>._))
                        .Invokes(c => metadata = c.GetArgument<Metadata<object>>(0));

                    /* Act */
                    _ = sut.Create(trigger);

                    /* Assert */
                    Assert.Same(trigger, metadata.Data);
                }
            }
        }

        public class Add : EFEventStoreFactoryTests
        {
            private readonly IEventStore store;
            private readonly MetadataId triggerId;

            public Add(DbDatabaseFixture fixture) : base(fixture)
            {
                var trigger = A.Dummy<object>();
                store = A.Fake<IEventStore>();
                triggerId = A.Dummy<MetadataId>();

                A.CallTo(() => idFactory.Create()).Returns(triggerId).Once();
                store = sut.Create(trigger);
            }

            [Fact]
            public void SetsEventMetadataId()
            {
                /* Arrange */
                EventMetadata<EventStub> metadata = null;
                var metadataId = A.Dummy<MetadataId>();
                A.CallTo(() => idFactory.Create()).Returns(metadataId);
                A.CallTo(() => db.Add(A<EventMetadata<EventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<EventStub>>(0));

                /* Act */
                _ = store.Add(A.Dummy<EventStub>());

                /* Assert */
                Assert.Same(metadataId, metadata.Id);
            }

            [Fact]
            public void SetsEventMetadataCorrelationId()
            {
                /* Arrange */
                EventMetadata<EventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<EventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<EventStub>>(0));

                /* Act */
                _ = store.Add(A.Dummy<EventStub>());

                /* Assert */
                Assert.Same(triggerId, metadata.CorrelationId);
            }

            [Fact]
            public void SetsEventMetadataToTheEvent()
            {
                /* Arrange */
                var e = A.Dummy<EventStub>();
                EventMetadata<EventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<EventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<EventStub>>(0));

                /* Act */
                _ = store.Add(e);

                /* Assert */
                Assert.Same(e, metadata.Event);
            }

            [Fact]
            public void ReturnsEventMetadataTrackingId()
            {
                /* Arrange */
                var e = new EventStub();
                EventMetadata<EventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<EventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<EventStub>>(0));

                /* Act */
                var id = store.Add(e);

                /* Assert */
                Assert.Same(id.Id, metadata.Id);
                Assert.Same(id.CausationId, metadata.CausationId);
                Assert.Same(id.CorrelationId, metadata.CorrelationId);
            }
        }
    }
}
