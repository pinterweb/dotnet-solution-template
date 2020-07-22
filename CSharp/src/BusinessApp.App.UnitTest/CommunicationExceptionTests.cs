namespace BusinessApp.Domain.UnitTest
{
    using System;
    using Xunit;
    using BusinessApp.App;
    using System.Collections;

    public class CommunicationExceptionTests
    {
        public class Constructor : CommunicationExceptionTests
        {
            [Fact]
            public void WithMessage_MappedToProperty()
            {
                /* Act */
                var ex = new CommunicationException("foo");

                /* Assert */
                Assert.Equal("foo", ex.Message);
            }

            [Fact]
            public void InnerException_MappedToProp()
            {
                /* Act */
                var inner = new Exception();
                var ex = new CommunicationException("foo", inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }

            [Fact]
            public void Data_AddsMessage()
            {
                /* Act */
                var ex = new CommunicationException("foo");

                /* Assert */
                var data = Assert.IsType<DictionaryEntry>(Assert.Single(ex.Data));
                Assert.Equal("", data.Key);
                Assert.Equal("foo", data.Value);
            }
        }
    }
}
