using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
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
        }
    }
}
