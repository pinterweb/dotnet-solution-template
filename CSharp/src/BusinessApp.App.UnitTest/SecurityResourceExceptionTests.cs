namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using System.Collections;

    public class SecurityResourceExceptionTests
    {
        public class Constructor : SecurityResourceExceptionTests
        {
            [Fact]
            public void WithoutResourceName_Throws()
            {
                /* Arrange */
                void shouldThrow() => new SecurityResourceException(null, "foo");

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void WithResourceName_SetsResourceName()
            {
                /* Act */
                var ex = new SecurityResourceException("foo", "bar");

                /* Assert */
                Assert.Equal("foo", ex.ResourceName);
            }

            [Fact]
            public void WithResourceName_SetsData()
            {
                /* Act */
                var ex = new SecurityResourceException("foo", "bar");

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("foo", data.Key);
                Assert.Equal("bar", data.Value);
            }

            [Fact]
            public void WithMessage_MappedToProp()
            {
                /* Act */
                var ex = new SecurityResourceException("foo", "bar");

                /* Assert */
                Assert.Equal("bar", ex.Message);
            }

            [Fact]
            public void WithInnerException_MappedToProp()
            {
                /* Act */
                var inner = new Exception();
                var ex = new SecurityResourceException("foo", "bar", inner);

                /* Assert */
                Assert.Equal(inner, ex.InnerException);
            }
        }

        public class IFormattableImpl : CommunicationExceptionTests
        {
            [Fact]
            public void ToString_MessageReturned()
            {
                /* Act */
                var ex = new CommunicationException("foo");

                /* Assert */
                Assert.Equal("foo", ex.ToString());
            }
        }
    }
}
