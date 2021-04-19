using Xunit;

namespace BusinessApp.WebApi.UnitTest
{
    public class BusinessAppWebApiExceptionTests
    {
        public class Constructor : BusinessAppWebApiExceptionTests
        {
            [Fact]
            public void WithMessage_MappedToProperty()
            {
                /* Act */
                var ex = new BusinessAppWebApiException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }
        }
    }
}
