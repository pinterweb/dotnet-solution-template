namespace BusinessApp.App.UnitTest
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FluentValidation.Results;
    using Xunit;

    public class FluentValidationValidatorTests
    {
        private readonly CancellationToken token;
        private readonly List<FluentValidation.IValidator<CommandStub>> validators;
        private readonly FluentValidationValidator<CommandStub> sut;
        private readonly CommandStub instance;

        public FluentValidationValidatorTests()
        {
            validators = new List<FluentValidation.IValidator<CommandStub>>();
            instance = new CommandStub();
            token = A.Dummy<CancellationToken>();
            sut = new FluentValidationValidator<CommandStub>(validators);
        }

        public class ValidateAsync : FluentValidationValidatorTests
        {
            [Fact]
            public async Task NotValidators_NoExceptionThrown()
            {
                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public async Task ValidResultFromAllValidators_NoExceptionThrown()
            {
                /* Arrange */
                var firstValidator = A.Fake<FluentValidation.IValidator<CommandStub>>();
                var secondValidator = A.Fake<FluentValidation.IValidator<CommandStub>>();
                A.CallTo(() => firstValidator.ValidateAsync(instance, A.Dummy<CancellationToken>()))
                    .Returns(new ValidationResult());
                A.CallTo(() => secondValidator.ValidateAsync(instance, A.Dummy<CancellationToken>()))
                    .Returns(new ValidationResult());
                validators.AddRange(new[] { firstValidator, secondValidator });

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                Assert.Null(ex);
            }

            [Fact]
            public async Task InValidResultsFromValidator_ModelExceptionGroupsByMember()
            {
                /* Arrange */
                var failures = new[]
                {
                    new ValidationFailure("foo", "bar"),
                    new ValidationFailure("foo", "lorem")
                };
                var validator = A.Fake<FluentValidation.IValidator<CommandStub>>();
                A.CallTo(() => validator.ValidateAsync(instance, A.Dummy<CancellationToken>()))
                    .Returns(new ValidationResult(failures));
                validators.Add(validator);

                /* Act */
                var ex = await Record.ExceptionAsync(async () => await sut.ValidateAsync(instance, token));

                /* Assert */
                var modelError = Assert.IsType<ModelValidationException>(ex);
                Assert.Collection(modelError,
                    e => Assert.Equal("foo", e.MemberName)
                );
                Assert.Collection(modelError,
                    e => Assert.Equal(new[] { "bar", "lorem" }, e.Errors)
                );
            }
        }
    }
}
