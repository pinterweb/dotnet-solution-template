using BusinessApp.Kernel;
using Xunit;

namespace BusinessApp.Kernel.UnitTest
{
    public class NullableExtensionsTests
    {
        public class Unwrap : NullableExtensionsTests
        {
            [Fact]
            public void HasObj_DoesNotThrow()
            {
                /* Arrange */
                var obj = new object();

                /* Act */
                var ex = Record.Exception(obj.Unwrap);

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public void NullObj_Throws()
            {
                /* Arrange */
                object obj = null;

                /* Act */
                var ex = Record.Exception(obj.Unwrap);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal("Object cannot be access because it is null", ex.Message);
            }
        }

        public class Expect : NullableExtensionsTests
        {
            [Fact]
            public void HasObj_DoesNotThrow()
            {
                /* Arrange */
                var obj = new object();

                /* Act */
                var ex = Record.Exception(() => obj.Expect("No error"));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public void NullObj_Throws()
            {
                /* Arrange */
                object obj = null;

                /* Act */
                var ex = Record.Exception(() => obj.Expect("Foobar"));

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal("Foobar: object cannot be null", ex.Message);
            }
        }
    }
}
