using System;
using Xunit;

namespace BusinessApp.Domain.UnitTest
{
    public class BusinessAppExceptionTests
    {
        public class Constructor : BusinessAppExceptionTests
        {
            [Fact]
            public void WithMessageArg_MappedToMessageProperty()
            {
                /* Act */
                var ex = new BusinessAppException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void WithInnerExceptionArg_MappedToInnerExceptionProperty()
            {
                /* Arrange */
                var inner = new Exception();

                /* Act */
                var ex = new BusinessAppException("foo", inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }
        }
    }
}
