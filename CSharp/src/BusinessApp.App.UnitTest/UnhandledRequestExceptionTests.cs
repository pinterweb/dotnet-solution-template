namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using FakeItEasy;

    public class UnhandledRequestExceptionTests
    {
        public class Constructor : UnhandledRequestExceptionTests
        {
            [Fact]
            public void OriginalException_CustomMessageUsed()
            {
                /* Arrange */
                var original = A.Dummy<Exception>();

                /* Act */
                var ex = new UnhandledRequestException(original);

                /* Assert */
                Assert.Equal(
                    "An unhandled exception has occurred during the request",
                    ex.Message
                );
            }

            [Fact]
            public void OriginalException_SetAsInnerExcecption()
            {
                /* Arrange */
                var original = A.Dummy<Exception>();

                /* Act */
                var ex = new UnhandledRequestException(original);

                /* Assert */
                Assert.Same(original, ex.InnerException);
            }
        }

        public class IFormattableImpl : UnhandledRequestExceptionTests
        {
            [Fact]
            public void ToString_ReturnsFormattedMessage()
            {
                /* Arrange */
                var original = new Exception("foobar");

                /* Act */
                var sut = new UnhandledRequestException(original);

                /* Assert */
                Assert.Equal(
                    "An unhandled exception has occurred during the request: foobar",
                    sut.ToString()
                );
            }

            [Fact]
            public void InnerExceptionFormattable_ReturnsFormattedMessage()
            {
                /* Arrange */
                var original = A.Fake<Exception>(opt => opt.Implements<IFormattable>());
                var provider = A.Fake<IFormatProvider>();
                A.CallTo(() => (original as IFormattable).ToString("z", provider)).Returns("lorem");

                /* Act */
                var sut = new UnhandledRequestException(original);

                /* Assert */
                Assert.Equal(
                    "An unhandled exception has occurred during the request: lorem",
                    sut.ToString("z", provider)
                );
            }
        }
    }
}
