namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using System.Collections;

    public class EntityNotFoundExceptionTests
    {
        public class Constructor : EntityNotFoundExceptionTests
        {
            [Fact]
            public void WithMessage_MappedToProperty()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void WithMessageAndEntityName_MessageCreated()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo", "bar");

                /* Assert */
                Assert.Equal("bar", ex.Message);
            }

            [Fact]
            public void WithNullMessageAndEntityName_MessageCreated()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo", null);

                /* Assert */
                Assert.Equal("foo not found", ex.Message);
            }

            [Fact]
            public void WithMessage_AddsToData()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo");

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("", data.Key);
                Assert.Equal("foo", data.Value);
            }

            [Fact]
            public void WithMessageAndEntityName_AddsData()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo", "bar");

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("foo", data.Key);
                Assert.Equal("bar", data.Value);
            }
        }

        public class IFormattableImpl : EntityNotFoundExceptionTests
        {
            [Fact]
            public void ToString_MessageReturned()
            {
                /* Act */
                var ex = new EntityNotFoundException("foo");

                /* Assert */
                Assert.Equal("foo", ex.ToString());
            }
        }
    }
}
