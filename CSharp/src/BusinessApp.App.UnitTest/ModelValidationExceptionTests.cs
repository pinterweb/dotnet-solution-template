namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using System.Collections;
    using System.Collections.Generic;
    using FakeItEasy;
    using BusinessApp.App;
    using System.Linq;

    public class ModelValidationExceptionTests
    {
        public class Constructor : ModelValidationExceptionTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null },
                new object[] { A.CollectionOfDummy<MemberValidationException>(0) },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(IEnumerable<MemberValidationException> errors)
            {
                /* Arrange */
                void shouldThrow() => new ModelValidationException("foo", errors);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void MessageArg_MappedToMessageProperty()
            {
                /* Act */
                var ex = new ModelValidationException("foomsg",
                    A.CollectionOfDummy<MemberValidationException>(1));

                /* Assert */
                Assert.Equal("foomsg", ex.Message);
            }

            [Fact]
            public void MemberErrosArg_MemberNamesMappedToDataKeys()
            {
                /* Act */
                var memberErrors = new[]
                {
                    new MemberValidationException("foobar", new[] { "bar" }),
                    new MemberValidationException("lorem", new[] { "ipsum", "dolor" }),
                };
                var ex = new ModelValidationException("foo", memberErrors);

                /* Assert */
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    e => Assert.Equal("foobar", e.Key),
                    e => Assert.Equal("lorem", e.Key)
                );
            }

            [Fact]
            public void MemberErrosArg_ErrorMessagesMappedToDataKeys()
            {
                /* Act */
                var memberErrors = new[]
                {
                    new MemberValidationException("foobar", new[] { "bar" }),
                    new MemberValidationException("lorem", new[] { "ipsum", "dolor" }),
                };
                var ex = new ModelValidationException("foo", memberErrors);

                /* Assert */
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    e => Assert.Equal(new[] { "bar" }, e.Value),
                    e => Assert.Equal(new[] { "ipsum", "dolor" }, e.Value)
                );
            }
        }
    }
}
