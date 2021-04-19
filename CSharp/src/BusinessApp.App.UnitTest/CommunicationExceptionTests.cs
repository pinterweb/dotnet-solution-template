using System;
using Xunit;

namespace BusinessApp.App.UnitTest
{
    public class CommunicationExceptionTests
    {
        public class Constructor : CommunicationExceptionTests
        {
            [Fact]
            public void MessageArg_MappedToProperty()
            {
                /* Act */
                var ex = new CommunicationException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void InnerExceptionArg_MappedToProp()
            {
                /* Act */
                var inner = new Exception();
                var ex = new CommunicationException("foo", inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }
        }
    }
}
