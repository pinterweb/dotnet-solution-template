namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using System.Linq;
    using System.Collections.Generic;

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
                var dictionary = Assert.IsType<Dictionary<string, string>>(ex.Data);
                var data = Assert.Single(dictionary);
                Assert.Contains("foo", data.Key);
                Assert.Contains("bar", data.Value);
            }
        }

        [Fact]
        public void InnerExceptions_HasSome_Returned()
        {
            /* Arrange */
            var innerInner = new Exception();
            var inner = new Exception("foo", innerInner);
            var ex = new Exception("bar", inner);

            /* Act */
            var inners = ex.Flatten();

            /* Assert */
            Assert.Equal(3, inners.Count());
            Assert.Contains(ex, inners);
            Assert.Contains(innerInner, inners);
            Assert.Contains(inner, inners);
        }
    }
}

