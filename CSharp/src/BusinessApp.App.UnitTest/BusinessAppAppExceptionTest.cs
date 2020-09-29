namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using BusinessApp.App;

    public class BusinessAppAppExceptionTests
    {
        public class Constructor : BusinessAppAppExceptionTests
        {
            [Fact]
            public void WithMessage_MappedToProperty()
            {
                /* Act */
                var ex = new BusinessAppAppException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }
        }
    }
}
