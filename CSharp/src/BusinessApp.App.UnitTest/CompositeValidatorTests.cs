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
        public async Task ValidateObject_NoValidators_NoExceptionThrown()
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
        public async Task ValidateObject_HasOneError_ValidationExceptionThrown()
        {
            /* Arrange */
            var expectedEx = new ValidationException("foo");
            A.CallTo(() => validators.First().ValidateAsync(instance))
                .Throws(expectedEx);
            async Task shouldThrow() => await sut.ValidateAsync(instance);

            /* Act */
            var actualEx = await Record.ExceptionAsync(shouldThrow);

            /* Assert */
            Assert.Same(expectedEx, actualEx);
        }

        [Fact]
        public async Task ValidateObject_HasManyErrors_AggregateExceptionThrown()
        {
            /* Arrange */
            A.CallTo(() => validators.First().ValidateAsync(instance))
                .Throws(new ValidationException("foo"));
            A.CallTo(() => validators.Last().ValidateAsync(instance))
                .Throws(new ValidationException("bar"));
            async Task shouldThrow() => await sut.ValidateAsync(instance);

            /* Act */
            var actualEx = await Record.ExceptionAsync(shouldThrow);

            /* Assert */
            Assert.IsType<AggregateException>(actualEx);
        }

        [Fact]
        public async Task ValidateObject_HasManyErrors_ContainsInnerValidationExceptions()
        {
            /* Arrange */
            var firstEx = new ValidationException("foo");
            var lastEx = new ValidationException("bar");
            A.CallTo(() => validators.First().ValidateAsync(instance))
                .Throws(firstEx);
            A.CallTo(() => validators.Last().ValidateAsync(instance))
                .Throws(lastEx);
            async Task shouldThrow() => await sut.ValidateAsync(instance);

            /* Act */
            var actualEx = await Record.ExceptionAsync(shouldThrow) as AggregateException;

            /* Assert */
            Assert.Contains(firstEx, actualEx.Flatten().InnerExceptions);
            Assert.Contains(lastEx, actualEx.Flatten().InnerExceptions);
        }
    }
}
