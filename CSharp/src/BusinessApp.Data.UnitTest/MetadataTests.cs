namespace BusinessApp.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using FakeItEasy;
    using BusinessApp.Domain;
    using Xunit;
    using BusinessApp.Test.Shared;

    public class MetadataTests
    {
        public class Constructor : MetadataTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[]
                {
                    null,
                    "foo",
                    A.Dummy<DomainEventStub>()
                },
                new object[]
                {
                    A.Dummy<MetadataId>(),
                    null,
                    A.Dummy<DomainEventStub>()
                },
                new object[]
                {
                    A.Dummy<MetadataId>(),
                    "",
                    A.Dummy<DomainEventStub>()
                },
                new object[]
                {
                    A.Dummy<MetadataId>(),
                    "foo",
                    null
                },
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(MetadataId id, string u, DomainEventStub t)
            {
                /* Arrange */
                void shouldThrow() => new Metadata<DomainEventStub>(id, u,
                    A.Dummy<MetadataType>(), t);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void DataArg_DataSetPropSet()
            {
                /* Arrange */
                var data = A.Fake<DomainEventStub>();
                A.CallTo(() => data.ToString()).Returns("foobar");

                /* Act */
                var sut = new Metadata<DomainEventStub>(A.Dummy<MetadataId>(), "foo",
                    A.Dummy<MetadataType>(), data);

                /* Assert */
                Assert.Equal("foobar", sut.DataSetName);
            }

            [Fact]
            public void DataArg_DataPropSet()
            {
                /* Arrange */
                var data = A.Dummy<DomainEventStub>();

                /* Act */
                var sut = new Metadata<DomainEventStub>(A.Dummy<MetadataId>(), "foo",
                    A.Dummy<MetadataType>(), data);

                /* Assert */
                Assert.Same(data, sut.Data);
            }

            [Fact]
            public void IdArg_IdPropertySet()
            {
                /* Arrange */
                var id = A.Dummy<MetadataId>();

                /* Act */
                var sut = new Metadata<DomainEventStub>(id, "foo", A.Dummy<MetadataType>(),
                    A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Same(id, sut.Id);
            }

            [Fact]
            public void UsernameArg_UsernamePropertySet()
            {
                /* Arrange */
                var id = A.Dummy<MetadataId>();

                /* Act */
                var sut = new Metadata<DomainEventStub>(A.Dummy<MetadataId>(), "foo",
                    A.Dummy<MetadataType>(), A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Equal("foo", sut.Username);
            }

            [Fact]
            public void TypeArg_TypeNamePropertySet()
            {
                /* Arrange */
                var type = MetadataType.Request;

                /* Act */
                var sut = new Metadata<DomainEventStub>(A.Dummy<MetadataId>(), "foo", type,
                     A.Dummy<DomainEventStub>());

                /* Assert */
                Assert.Equal("Request", sut.TypeName);
            }

            [Fact]
            public void OccurredUtcPropertySetToNow()
            {
                /* Arrange */
                var before = DateTimeOffset.UtcNow;

                /* Act */
                var sut = new Metadata<DomainEventStub>(A.Dummy<MetadataId>(), "foo",
                    A.Dummy<MetadataType>(), A.Dummy<DomainEventStub>());

                /* Assert */
                var after = DateTimeOffset.UtcNow;
                Assert.True(before <= sut.OccurredUtc && after >= sut.OccurredUtc);
            }
        }
    }
}
