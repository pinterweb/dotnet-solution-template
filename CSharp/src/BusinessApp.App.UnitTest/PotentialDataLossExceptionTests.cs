namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using System.Collections;

    public class PotentialDataLossExceptionTests
    {
        public class MessageConstructor : PotentialDataLossExceptionTests
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WithoutMessage_Throws(string msg)
            {
                /* Arrange */
                void shouldThrow() => new PotentialDataLossException(msg);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void WithMessage_MappedToProp()
            {
                /* Act */
                var ex = new PotentialDataLossException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void WithMessage_SetsData()
            {
                /* Act */
                var ex = new PotentialDataLossException("foo");

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("", data.Key);
                Assert.Equal("foo", data.Value);
            }
        }

        public class MessageInnerExceptionConstructor : PotentialDataLossExceptionTests
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            public void WithoutMessage_Throws(string msg)
            {
                /* Arrange */
                void shouldThrow() => new PotentialDataLossException(msg, null);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void WithMessage_MappedToProp()
            {
                /* Act */
                var ex = new PotentialDataLossException("foo", null);

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void WithInnerException_MappedToProp()
            {
                /* Arragange */
                var inner = new Exception();

                /* Act */
                var ex = new PotentialDataLossException("foo", inner);

                /* Assert */
                Assert.Equal(inner, ex.InnerException);
            }

            [Fact]
            public void WithMessage_SetsData()
            {
                /* Act */
                var ex = new PotentialDataLossException("foo", null);

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("", data.Key);
                Assert.Equal("foo", data.Value);
            }
        }
    }
}
