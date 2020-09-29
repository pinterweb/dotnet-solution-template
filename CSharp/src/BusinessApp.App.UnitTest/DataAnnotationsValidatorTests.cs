namespace BusinessApp.App.UnitTest
{
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    public class DataAnnotationsValidatorTests
    {
        private readonly CancellationToken token;
        private readonly DataAnnotationsValidator<DataAnnotatedCommandStub> sut;

        public DataAnnotationsValidatorTests()
        {
            token = A.Dummy<CancellationToken>();
            sut = new DataAnnotationsValidator<DataAnnotatedCommandStub>();
        }

        public class ValidateAsync : DataAnnotationsValidatorTests
        {
            [Fact]
            public async Task InstanceIsValid_NoExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Foo = new string('a', 10) };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public async Task InstanceHasErrors_ModelExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub
                {
                    CompareToBar = null,
                    Bar = null,
                    Foo = new string('a', 11)
                };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.IsType<ModelValidationException>(ex);
                Assert.Equal(
                    "The model did not pass validation. See erros for more details",
                    ex.Message
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
                   CompareToBar = null,
                   Bar = null,
                   Foo = new string('a', 11)
                };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(ex);
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
                    CompareToBar = null,
                    Bar = null,
                    Foo = new string('a', 11)
                };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(ex);
                var memberError = Assert.Single(modelError, e => e.MemberName == memberName);
                Assert.Contains(associatedMsg, memberError.Errors);
            }

            [Fact]
            public async Task NoMemberNames_ExceptionThrown()
            {
                /* Arrange */
                var instance = new DataAnnotatedCommandStub { Bar = "a", CompareToBar = "aaaa" };

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var modelError = Assert.IsType<BusinessAppAppException>(ex);
                Assert.Equal(
                    "All errors must have a member name. " +
                    "If the attribute does not support this, please create or extend the attribute",
                    ex.Message
                );
            }
        }
    }
}
