namespace BusinessApp.App.UnitTest
{
    using System;
    using BusinessApp.Domain;
    using Xunit;

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
                Assert.IsType<BusinessAppException>(ex);
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
    }
}
