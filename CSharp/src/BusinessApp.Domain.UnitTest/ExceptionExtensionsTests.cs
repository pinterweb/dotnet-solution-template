using System;
using Xunit;
using BusinessApp.Domain;
using System.Linq;

namespace BusinessApp.Domain.UnitTest
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void Flatten_NoInnerExceptions_ContainsSelf()
        {
            /* Arrange */
            var ex = new Exception();

            /* Act */
            var inners = ex.Flatten();

            /* Assert */
            var inner = Assert.Single(inners);
            Assert.Same(ex, inner);
        }

        [Fact]
        public void Flatten_InnerExceptions_ReturnsAll()
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
            Assert.Contains(inner, inners);
            Assert.Contains(innerInner, inners);
        }
    }
}
