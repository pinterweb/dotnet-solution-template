using Xunit;
using FakeItEasy;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class EnvelopeContractTests
    {
        public class Constructor : EnvelopeContractTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null, A.Dummy<Pagination>() },
                new object[] { A.CollectionOfDummy<CommandStub>(0), null },
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(IEnumerable<CommandStub> d, Pagination p)
            {
                /* Arrange */
                void shouldThrow() => new EnvelopeContract<CommandStub>(d, p);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }

            [Fact]
            public void SetsData()
            {
                /* Arrange */
                var stub = new CommandStub();

                /* Act */
                var sut = new EnvelopeContract<CommandStub>(new[] { stub }, A.Dummy<Pagination>());

                /* Assert */
                Assert.Collection(sut.Data, d => Assert.Same(stub, d));
            }

            [Fact]
            public void SetsPagination()
            {
                /* Arrange */
                var page = new Pagination();

                /* Act */
                var sut = new EnvelopeContract<CommandStub>(A.CollectionOfDummy<CommandStub>(0), page);

                /* Assert */
                Assert.Same(page, sut.Pagination);
            }
        }

        public class IEnumerableImpl : EnvelopeContractTests
        {
            [Fact]
            public void ReturnsDataEnumerator()
            {
                /* Arrange */
                var expectEnumerator = A.Fake<IEnumerator<CommandStub>>();
                var data = A.Fake<IEnumerable<CommandStub>>();
                var sut = new EnvelopeContract<CommandStub>(data, A.Dummy<Pagination>());
                A.CallTo(() => data.GetEnumerator()).Returns(expectEnumerator);

                /* Act */
                var actualEnumerator = sut.GetEnumerator();

                /* Assert */
                Assert.Same(expectEnumerator, actualEnumerator);
            }
        }
    }
}
