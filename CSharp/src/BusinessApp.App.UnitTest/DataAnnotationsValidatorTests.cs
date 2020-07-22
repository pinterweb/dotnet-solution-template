namespace BusinessApp.App.UnitTest
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class DataAnnotationsValidatorTests
    {
        private readonly CancellationToken token;
        private readonly IValidator<DataAnnotatedCommandStub> sut;

        public DataAnnotationsValidatorTests()
        {
            token = A.Dummy<CancellationToken>();
            sut = new DataAnnotationsValidator<DataAnnotatedCommandStub>();
        }

        public class ValidateObject_WithOneError : DataAnnotationsValidatorTests
        {
            [Fact]
            public async Task ValidationExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Foo = new string('a', 11) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.IsType<ValidationException>(ex);
                Assert.Equal("The field Foo must be a string with a maximum length of 10.", ex.Message);
            }
        }

        public class ValidateObject_WithNoError : DataAnnotationsValidatorTests
        {
            [Fact]
            public async Task NoExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Foo = new string('a', 10) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.Null(ex);
            }
        }

        public class ValidateObject_WithMultipleErrors : DataAnnotationsValidatorTests
        {
            DataAnnotatedCommandStub instance;

            public ValidateObject_WithMultipleErrors()
            {
                instance = new DataAnnotatedCommandStub { Bar = null, Foo = new string('a', 11) };
            }

            [Fact]
            public async Task AggregateExceptionThrown()
            {
                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.IsType<AggregateException>(ex);
            }

            [Fact]
            public async Task ContainsLenghtInnerValidationException()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Bar = null, Foo = new string('a', 11) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var aggregateEx = Assert.IsType<AggregateException>(ex);
                Assert.Equal(
                    "The field Foo must be a string with a maximum length of 10.",
                    aggregateEx.Flatten().InnerExceptions.First().Message);
            }

            [Fact]
            public async Task ContainsRequiredInnerValidationException()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Bar = null, Foo = new string('a', 11) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var aggregateEx = Assert.IsType<AggregateException>(ex);
                Assert.Equal(
                    "The Bar field is required.",
                    aggregateEx.Flatten().InnerExceptions.Last().Message);
            }
        }
    }
}
