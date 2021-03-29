namespace BusinessApp.WebApi.UnitTest.ProblemDetails
{
    using Xunit;
    using BusinessApp.WebApi.ProblemDetails;
    using FakeItEasy;
    using Microsoft.Extensions.Localization;
    using System.Collections.Generic;
    using System;

    public class LocalizedProblemDetailFactoryTests
    {
        private readonly LocalizedProblemDetailFactory sut;
        private readonly IProblemDetailFactory inner;
        private readonly IStringLocalizer localizer;

        public LocalizedProblemDetailFactoryTests()
        {
            inner = A.Fake<IProblemDetailFactory>();
            localizer = A.Fake<IStringLocalizer>();

            sut = new LocalizedProblemDetailFactory(inner, localizer);
        }

        public class Constructor : ProblemDetailTests
        {
            public static IEnumerable<object[]> InvalidArgs => new[]
            {
                new object[] { A.Dummy<IProblemDetailFactory>(), null },
                new object[] { null, A.Dummy<IStringLocalizer>() }
            };

            [Theory, MemberData(nameof(InvalidArgs))]
            public void InvalidArgs_ExceptionThrown(IProblemDetailFactory f,
                IStringLocalizer l)
            {
                /* Arrange */
                void shouldThrow() => new LocalizedProblemDetailFactory(f, l);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }
        }

        public class Create : LocalizedProblemDetailFactoryTests
        {
            [Fact]
            public void StringExtension_TranslationCalled()
            {
                /* Arrange */
                var exception = A.Dummy<Exception>();
                var localizedStr = new LocalizedString("bar", "ipsum");
                var problem = new ProblemDetail(400)
                {
                    { "foo", "bar" }
                };
                A.CallTo(() => localizer["bar"]).Returns(localizedStr);
                A.CallTo(() => inner.Create(exception)).Returns(problem);

                /* Act */
                var localizedProblem = sut.Create(exception);

                /* Assert */
                Assert.Equal("ipsum", localizedProblem["foo"]);
            }

            [Fact]
            public void DictionaryExtension_TranslationCalled()
            {
                /* Arrange */
                var exception = A.Dummy<Exception>();
                var localizedStr = new LocalizedString("bar", "ipsum");
                var dictionary = new Dictionary<string, string>
                {
                    { "dolor", "ipsit" }
                };
                var problem = new ProblemDetail(400)
                {
                    { "foo", dictionary }
                };
                A.CallTo(() => localizer["ipsit"]).Returns(localizedStr);
                A.CallTo(() => inner.Create(exception)).Returns(problem);

                /* Act */
                var localizedProblem = sut.Create(exception);

                /* Assert */
                Assert.Collection((IDictionary<string, object>)localizedProblem["foo"],
                    kvp => Assert.Equal("ipsum", kvp.Value));
            }

            [Fact]
            public void ArrayExtension_TranslationCalled()
            {
                /* Arrange */
                var exception = A.Dummy<Exception>();
                var firstLocalizedStr = new LocalizedString("bar", "ipsum");
                var secondLocalizedStr = new LocalizedString("ipsit", "lorem");
                var problem = new ProblemDetail(400)
                {
                    { "foo", new[] { "bar", "dolor" } }
                };
                A.CallTo(() => localizer["bar"]).Returns(firstLocalizedStr);
                A.CallTo(() => localizer["dolor"]).Returns(secondLocalizedStr);
                A.CallTo(() => inner.Create(exception)).Returns(problem);

                /* Act */
                var localizedProblem = sut.Create(exception);

                /* Assert */
                var items = Assert.IsType<List<object>>(localizedProblem["foo"]);
                Assert.Collection(items,
                    val => Assert.Equal("ipsum", val),
                    val => Assert.Equal("lorem", val));
            }

            [Fact]
            public void ArrayInDictionaryExtension_TranslationCalled()
            {
                /* Arrange */
                var exception = A.Dummy<Exception>();
                var firstLocalizedStr = new LocalizedString("bar", "ipsum");
                var secondLocalizedStr = new LocalizedString("ipsit", "lorem");
                var dictionary = new Dictionary<string, string[]>
                {
                    { "dolor", new[] { "bar", "dolor" } }
                };
                var problem = new ProblemDetail(400)
                {
                    { "foo", dictionary }
                };
                A.CallTo(() => localizer["bar"]).Returns(firstLocalizedStr);
                A.CallTo(() => localizer["dolor"]).Returns(secondLocalizedStr);
                A.CallTo(() => inner.Create(exception)).Returns(problem);

                /* Act */
                var localizedProblem = sut.Create(exception);

                /* Assert */
                var nested = (IDictionary<string, object>)localizedProblem["foo"];
                var items = Assert.IsType<List<object>>(nested["dolor"]);
                Assert.Collection(items,
                    val => Assert.Equal("ipsum", val),
                    val => Assert.Equal("lorem", val));
            }
        }
    }
}
