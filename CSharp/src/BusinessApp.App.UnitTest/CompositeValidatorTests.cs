namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using System.Collections.Generic;
    using FakeItEasy;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Threading;
    using BusinessApp.Domain;

    public class ValidationStub {  }

    public class CompositeValidatorTests
    {
        private readonly CancellationToken token;
        private readonly IEnumerable<IValidator<ValidationStub>> validators;
        private readonly ValidationStub instance;
        private CompositeValidator<ValidationStub> sut;

        public CompositeValidatorTests()
        {
            token = A.Dummy<CancellationToken>();
            instance = new ValidationStub();
            validators = new[]
            {
                A.Fake<IValidator<ValidationStub>>(),
                A.Fake<IValidator<ValidationStub>>(),
            };
            sut = new CompositeValidator<ValidationStub>(validators);
        }

        public class Constructor: CompositeValidatorTests
        {
            [Fact]
            public void InvalidArgs_ExceptionThrown()
            {
                /* Arrange */
                void shouldThrow() => new CompositeValidator<ValidationStub>(null);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BadStateException>(ex);
            }
        }

        public class ValidateAsync : CompositeValidatorTests
        {
            [Fact]
            public async Task EmptyValidators_OkResultReturned()
            {
                /* Arrange */
                sut = new CompositeValidator<ValidationStub>(new IValidator<ValidationStub>[0]);

                /* Act */
                var result = await sut.ValidateAsync(A.Dummy<ValidationStub>(), token);

                /* Assert */
                Assert.Equal(Result.Ok, result);
            }

            [Fact]
            public async Task MultipleValidators_FirstErrorReturned()
            {
                /* Arrange */
                var error = Result.Error($"foobar");
                A.CallTo(() => validators.First().ValidateAsync(instance, token))
                    .Returns(Result.Error($"foobar"));
                A.CallTo(() => validators.Last().ValidateAsync(instance, token))
                    .Returns(Result.Ok);

                /* Act */
                var result = await sut.ValidateAsync(instance, token);

                /* Assert */
                Assert.Equal(error, result);
            }

            [Fact]
            public async Task MultipleValidatorsAllOkResults_OkResultReturned()
            {
                /* Arrange */
                A.CallTo(() => validators.First().ValidateAsync(instance, token))
                    .Returns(Result.Ok);
                A.CallTo(() => validators.Last().ValidateAsync(instance, token))
                    .Returns(Result.Ok);

                /* Act */
                var result = await sut.ValidateAsync(instance, token);

                /* Assert */
                Assert.Equal(Result.Ok, result);
            }
        }
    }
}
