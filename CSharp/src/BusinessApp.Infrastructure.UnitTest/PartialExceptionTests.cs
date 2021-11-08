using System;
using Xunit;
using FakeItEasy;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class PartialExceptionTests
    {
        public class Constructor : PartialExceptionTests
        {
            [Fact]
            public void DataArgAddedToDictionary()
            {
                /* Arrange */
                var data = new object();
                var inner = A.Dummy<Exception>();

                /* Act */
                var sut = new PartialException(inner, data);

                /* Assert */
                Assert.Same(data, sut.Data["Data"]);
            }
        }
    }
}
