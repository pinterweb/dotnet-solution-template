namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using System.Collections;
    using System.Collections.Generic;
    using FakeItEasy;
    using BusinessApp.App;

    public class MemberValidationExceptionTests
    {
        public class Constructor : MemberValidationExceptionTests
        {
            public static IEnumerable<object[]> InvalidCtorArgs => new[]
            {
                new object[] { null, A.CollectionOfDummy<string>(1) },
                new object[] { "", A.CollectionOfDummy<string>(1) },
                new object[] { "foo", null },
                new object[] { "foo", A.CollectionOfDummy<string>(0) },
            };

            [Theory, MemberData(nameof(InvalidCtorArgs))]
            public void InvalidCtorArgs_ExceptionThrown(string memberName,
                IEnumerable<string> errors)
            {
                /* Arrange */
                void shouldThrow() => new MemberValidationException(memberName, errors);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void MessageProperty_HardCodedInCtor()
            {
                /* Act */
                var ex = new MemberValidationException("foo", A.CollectionOfDummy<string>(1));

                /* Assert */
                Assert.Equal("'foo' failed validation. See errors for more details", ex.Message);
            }

            [Fact]
            public void MemberNameArg_MappedToProperty()
            {
                /* Act */
                var ex = new MemberValidationException("foo", A.CollectionOfDummy<string>(1));

                /* Assert */
                Assert.Equal("foo", ex.MemberName);
            }

            [Fact]
            public void ErrorsArg_MappedToProperty()
            {
                /* Act */
                var ex = new MemberValidationException("foo", new[] { "bar", "lorem" });

                /* Assert */
                Assert.Collection(ex.Errors,
                    e => Assert.Equal("bar", e),
                    e => Assert.Equal("lorem", e)
                );
            }

            [Fact]
            public void DataProperty_MemberNameAndErrorsAdded()
            {
                /* Act */
                var ex = new MemberValidationException("foo", new[] { "bar", "lorem" });

                /* Assert */
                var entry = Assert.Single(ex.Data);
                var dictEntry = Assert.IsType<DictionaryEntry>(entry);
                Assert.Equal("foo", dictEntry.Key);
                Assert.Equal(new[] { "bar", "lorem" }, dictEntry.Value);
            }
        }
    }
}
