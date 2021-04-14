namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.App;

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
