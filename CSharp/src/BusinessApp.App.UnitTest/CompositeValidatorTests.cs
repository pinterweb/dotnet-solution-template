namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using System.Collections.Generic;
    using FakeItEasy;
    using System.Threading.Tasks;
    using System.Linq;

    public class ValidationStub {  }

    public class CompositeValidatorTests
    {
        private readonly IEnumerable<IValidator<ValidationStub>> validators;
        private readonly ValidationStub instance;
        private CompositeValidator<ValidationStub> sut;

        public CompositeValidatorTests()
        {
            instance = new ValidationStub();
            validators = new[]
            {
                A.Fake<IValidator<ValidationStub>>(),
                A.Fake<IValidator<ValidationStub>>(),
            };
            sut = new CompositeValidator<ValidationStub>(validators);
        }

        [Fact]
        public void Constructor_ExceptionThrown()
        {
            /* Arrange */
            void shouldThrow() => new CompositeValidator<ValidationStub>(null);

            /* Act */
            var ex = Record.Exception(shouldThrow);

            /* Assert */
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public async Task ValidateAsync_NullValidators_NoExceptionThrown()
        {
            /* Arrange */
            sut = new CompositeValidator<ValidationStub>(new IValidator<ValidationStub>[0]);
            async Task shouldNotThrow() => await sut.ValidateAsync(A.Dummy<ValidationStub>());

            /* Act */
            var ex = await Record.ExceptionAsync(shouldNotThrow);

            /* Assert */
            Assert.Null(ex);
        }

        [Fact]
        public async Task ValidateAsync_MultipleValidators_AllCalled()
        {
            /* Arrange */
            async Task shouldNotThrow() => await sut.ValidateAsync(instance);

            /* Act */
            var ex = await Record.ExceptionAsync(shouldNotThrow);

            /* Assert */
            A.CallTo(() => validators.First().ValidateAsync(instance))
                .MustHaveHappenedOnceExactly().Then(
                    A.CallTo(() => validators.Last().ValidateAsync(instance))
                        .MustHaveHappenedOnceExactly());
        }
    }
}
