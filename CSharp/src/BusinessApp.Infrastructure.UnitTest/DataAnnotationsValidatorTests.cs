using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Domain;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class DataAnnotatedCommandStub
    {
        [NoMember]
        public string NoMember { get; set; }

        [NoErrMsg]
        public string NoErrMsg { get; set; }

        [StringLength(10)]
        public string Foo { get; set; }

        [Required]
        public string Bar { get; set; } = "lorem";

        private class NoErrMsgAttribute : ValidationAttribute
        {
            public override bool IsValid(object value) => value == null;

            public override string FormatErrorMessage(string name) => null;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null) return base.IsValid(value, validationContext);

                var result = base.IsValid(value, validationContext);

                result.ErrorMessage = null;

                return result;
            }
        }

        private class NoMemberAttribute : ValidationAttribute
        {
            public override bool IsValid(object value) => value == null;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value == null) return base.IsValid(value, validationContext);

                validationContext.MemberName = null;
                var result = base.IsValid(value, validationContext);
                return result;
            }
        }
    }

    public class DataAnnotationsValidatorTests
    {
        private readonly CancellationToken cancelToken;
        private readonly DataAnnotationsValidator<DataAnnotatedCommandStub> sut;

        public DataAnnotationsValidatorTests()
        {
            cancelToken = A.Dummy<CancellationToken>();
            sut = new DataAnnotationsValidator<DataAnnotatedCommandStub>();
        }

        public class ValidateAsync : DataAnnotationsValidatorTests
        {
            [Fact]
            public async Task InstanceIsValid_OkResultReturned()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Foo = new string('a', 10) };

                /* Act */
                var result = await sut.ValidateAsync(instance, cancelToken);

                /* Assert */
                Assert.Equal(Result.OK, result);
            }

            [Fact]
            public async Task InstanceHasErrors_ModelExceptionUnwrapped()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub
                {
                    Bar = null,
                    Foo = new string('a', 11)
                };

                /* Act */
                var result = await sut.ValidateAsync(instance, cancelToken);

                /* Assert */
                var error = Assert.IsType<ModelValidationException>(result.UnwrapError());
                Assert.Equal(
                    "The model did not pass validation. See erros for more details",
                    error.Message
                );
            }

            [Theory]
            [InlineData("Bar")]
            [InlineData("Foo")]
            public async Task InstanceHasManyErrors_ModelExceptionHasMemberNames(string memberName)
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub
                {
                   Bar = null,
                   Foo = new string('a', 11)
                };

                /* Act */
                var result = await sut.ValidateAsync(instance, cancelToken);

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(result.UnwrapError());
                Assert.Single(modelError, e => e.MemberName == memberName);
            }

            [Theory]
            [InlineData("Bar", "The Bar field is required.")]
            [InlineData("Foo", "The field Foo must be a string with a maximum length of 10.")]
            public async Task InstanceHasManyErrors_ModelExceptionHasMemberMessage(string memberName,
                string associatedMsg)
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub
                {
                    Bar = null,
                    Foo = new string('a', 11)
                };

                /* Act */
                var result = await sut.ValidateAsync(instance, cancelToken);

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(result.UnwrapError());
                var memberError = Assert.Single(modelError, e => e.MemberName == memberName);
                Assert.Contains(associatedMsg, memberError.Errors);
            }

            [Fact]
            public async Task NoMemberNames_ExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { NoMember = "foo" };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, cancelToken));

                /* Assert */
                var modelError = Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(
                    "All errors must have a member name. " +
                    "If the attribute does not support this, please create or extend the attribute",
                    ex.Message
                );
            }

            [Fact]
            public async Task NoErrorMessage_ExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { NoErrMsg = "foo" };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, cancelToken));

                /* Assert */
                var modelError = Assert.IsType<BusinessAppException>(ex);
                Assert.Equal("All errors must have an error message.", ex.Message);
            }
        }
    }
}
