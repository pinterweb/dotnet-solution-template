namespace BusinessApp.Domain.UnitTest
{
    using System;
    using Xunit;

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

        public class IFormattableImpl : BusinessAppExceptionTests
        {
            [Fact]
            public void ToString_MessageReturned()
            {
                /* Act */
                var ex = new BusinessAppException("foo");

                /* Assert */
                Assert.Equal("foomsg", ex.ToString());
            }
        }
    }
}
