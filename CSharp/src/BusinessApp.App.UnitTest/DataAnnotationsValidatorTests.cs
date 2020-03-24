namespace BusinessApp.App.UnitTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using BusinessApp.Domain;

    public class DataAnnotationsValidatorTests
    {
        private readonly IValidator<DataAnnotatedCommandStub> sut;

        public DataAnnotationsValidatorTests()
        {
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
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance));

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
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance));

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
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance));

                /* Assert */
                Assert.IsType<AggregateException>(ex);
            }

            [Fact]
            public async Task ContainsLenghtInnerValidationException()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Bar = null, Foo = new string('a', 11) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance)) as AggregateException;

                /* Assert */
                Assert.Equal(
                    "The field Foo must be a string with a maximum length of 10.",
                    ex.Flatten().InnerExceptions.First().Message);
            }

            [Fact]
            public async Task ContainsRequiredInnerValidationException()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Bar = null, Foo = new string('a', 11) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance)) as AggregateException;

                /* Assert */
                Assert.Equal(
                    "The Bar field is required.",
                    ex.Flatten().InnerExceptions.Last().Message);
            }
        }
    }
}
