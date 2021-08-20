using System.Collections.Generic;
using BusinessApp.Kernel;
using BusinessApp.WebApi.ProblemDetails;
using Xunit;

namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    public class UnsupportedMediaTypeExceptionTests
    {
        public class Constructor : UnsupportedMediaTypeExceptionTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { null },
                new object[] { "" },
                new object[] { " " },
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(string m)
            {
                /* Arrange */
                void shouldThrow() => new UnsupportedMediaTypeException(m);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
            }
        }
    }
}
