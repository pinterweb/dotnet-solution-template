namespace BusinessApp.Data.IntegrationTest
{
    using FakeItEasy;
    using System.Collections.Generic;
    using Xunit;
    using BusinessApp.Domain;
    using BusinessApp.Test.Shared;
    using System.Security.Principal;
    using BusinessApp.App;

    [Collection(nameof(DatabaseCollection))]
    public class EFEventStoreFactoryTests
    {
        private readonly EFEventStoreFactory sut;
        private readonly IEntityIdFactory<MetadataId> idFactory;
        private readonly IPrincipal user;
        private readonly BusinessAppDbContext db;

        public EFEventStoreFactoryTests()
        {
            idFactory = A.Fake<IEntityIdFactory<MetadataId>>();
            db = A.Fake<BusinessAppDbContext>();
            user = A.Fake<IPrincipal>();

            sut = new EFEventStoreFactory(db, idFactory, user);

            A.CallTo(() => user.Identity.Name).Returns("foo");
        }

        public class Constructor : EFEventStoreFactoryTests
        {
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
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class Create : EFEventStoreFactoryTests
        {
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
                var _ = sut.Create(A.Dummy<object>());

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
                var _ = sut.Create(A.Dummy<object>());

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
                var _ = sut.Create(A.Dummy<object>());

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
                var _ = sut.Create(trigger);

                /* Assert */
                Assert.Same(trigger, metadata.Data);
            }
        }

        public class Add : EFEventStoreFactoryTests
        {
            private readonly IEventStore store;
            private readonly MetadataId triggerId;

            public Add()
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
                EventMetadata<DomainEventStub> metadata = null;
                var metadataId = A.Dummy<MetadataId>();
                A.CallTo(() => idFactory.Create()).Returns(metadataId);
                A.CallTo(() => db.Add(A<EventMetadata<DomainEventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<DomainEventStub>>(0));

                /* Act */
                var _ = store.Add(A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Same(metadataId, metadata.Id);
            }

            [Fact]
            public void SetsEventMetadataCorrelationId()
            {
                /* Arrange */
                EventMetadata<DomainEventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<DomainEventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<DomainEventStub>>(0));

                /* Act */
                var _ = store.Add(A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Same(triggerId, metadata.CorrelationId);
            }

            [Fact]
            public void SetsEventMetadataCausationIdFromTriggerMetadataId()
            {
                /* Arrange */
                EventMetadata<DomainEventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<DomainEventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<DomainEventStub>>(0));

                /* Act */
                var _ = store.Add(A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Same(triggerId, metadata.CausationId);
            }

            [Fact]
            public void SetsEventMetadataToTheEvent()
            {
                /* Arrange */
                var e = A.Dummy<DomainEventStub>();
                EventMetadata<DomainEventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<DomainEventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<DomainEventStub>>(0));

                /* Act */
                var _ = store.Add(e);

                /* Assert */
                Assert.Same(e, metadata.Event);
            }

            [Fact]
            public void ReturnsEventMetadataTrackingId()
            {
                /* Arrange */
                var e = new DomainEventStub();
                EventMetadata<DomainEventStub> metadata = null;
                A.CallTo(() => db.Add(A<EventMetadata<DomainEventStub>>._))
                    .Invokes(c => metadata = c.GetArgument<EventMetadata<DomainEventStub>>(0));

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
