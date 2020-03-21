namespace ShelLife.Domain.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.Domain;
    using System.Linq;

    public class ExceptionExtensionsTests
    {
        [Fact]
        public void InnerExceptions_HasNone_ReturnsEmpty()
        {
            /* Arrange */
            var ex = new Exception();

            /* Act */
            var inners = ex.InnerExceptions();

            /* Assert */
            Assert.Empty(inners);
        }

        [Fact]
        public void InnerExceptions_HasSome_Returned()
        {
            /* Arrange */
            var innerInner = new Exception();
            var inner = new Exception("foo", innerInner);
            var ex = new Exception("bar", inner);

            /* Act */
            var inners = ex.InnerExceptions();

            /* Assert */
            Assert.Equal(2, inners.Count());
            Assert.Contains(inner, inners);
            Assert.Contains(inner, inners);
        }
    }
}
