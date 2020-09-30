namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using System.Collections.Generic;
    using FakeItEasy;
    using System;
    using Microsoft.AspNetCore.Http;

    public class ProblemDetailFactoryHttpDecoratorTests
    {
        private readonly IProblemDetailFactory inner;
        private readonly IHttpContextAccessor accessor;
        private readonly HttpContext context;
        public readonly ProblemDetailFactoryHttpDecorator sut;

        public ProblemDetailFactoryHttpDecoratorTests()
        {
            inner = A.Fake<IProblemDetailFactory>();
            accessor = A.Fake<IHttpContextAccessor>();
            sut = new ProblemDetailFactoryHttpDecorator(inner, accessor);

            A.CallTo(() => accessor.HttpContext)
                .Returns(context = A.Fake<HttpContext>());
        }

        public class Constructor : ProblemDetailFactoryHttpDecoratorTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.Dummy<IHttpContextAccessor>() },
                new object[] { A.Dummy<IProblemDetailFactory>(), null },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IProblemDetailFactory i, IHttpContextAccessor a)
            {
                /* Arrange */
                void shouldThrow() => new ProblemDetailFactoryHttpDecorator(i, a);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class Create : ProblemDetailFactoryHttpDecoratorTests
        {
            [Fact]
            public void NoHttpContext_ExceptionThrown()
            {
                /* Arrange */
                A.CallTo(() => accessor.HttpContext).Returns(null);

                /* Act */
                var ex = Record.Exception(() => sut.Create(A.Dummy<IFormattable>()));

                /* Assert */
                Assert.IsType<BusinessAppWebApiException>(ex);
                Assert.Equal(
                    "Cannot access decorate the `ProblemDetail`with Http specifics " +
                    "because we are not in a http context",
                    ex.Message
                );
            }

            [Fact]
            public void HttpContext_SetsStatusToProblemStatus()
            {
                /* Arrange */
                var error = A.Dummy<IFormattable>();
                var problem = new ProblemDetail(400);
                A.CallTo(() => inner.Create(error)).Returns(problem);

                /* Act */
                var _ = sut.Create(error);

                /* Assert */
                A.CallToSet(() => context.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void ProblemResult_InnerValueReturned()
            {
                /* Arrange */
                var error = A.Dummy<IFormattable>();
                var actualProblem = new ProblemDetail(400);
                A.CallTo(() => inner.Create(error)).Returns(actualProblem);

                /* Act */
                var returnedProblem = sut.Create(error);

                /* Assert */
                Assert.Equal(actualProblem, returnedProblem);
            }
        }
    }
}
