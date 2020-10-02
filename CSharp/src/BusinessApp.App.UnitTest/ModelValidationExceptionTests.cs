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
            public void InvalidMessageMembersCtorArgs_ExceptionThrown(IEnumerable<MemberValidationException> errors)
            {
                /* Arrange */
                void shouldThrow() => new ModelValidationException("foo", errors);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.NotNull(ex);
            }

            [Fact]
            public void WithMessage_MappedToMessageProperty()
            {
                /* Act */
                var ex = new ModelValidationException("foomsg");

                /* Assert */
                Assert.Equal("foomsg", ex.Message);
            }

            [Fact]
            public void WithMessage_MembersSetToEmtpyList()
            {
                /* Act */
                var ex = new ModelValidationException("foomsg");

                /* Assert */
                Assert.Empty(ex);
            }

            [Fact]
            public void WithMessageAndMembers_MappedToMessageProperty()
            {
                /* Act */
                var ex = new ModelValidationException("foomsg",
                    A.CollectionOfDummy<MemberValidationException>(1));

                /* Assert */
                Assert.Equal("foomsg", ex.Message);
            }

            [Fact]
            public void WithMessageAndMembers_MemberNamesMappedToDataKeys()
            {
                /* Arrange */
                var memberErrors = new[]
                {
                    new MemberValidationException("foobar", new[] { "bar" }),
                    new MemberValidationException("lorem", new[] { "ipsum", "dolor" }),
                };

                /* Act */
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
                /* Arrange */
                var memberErrors = new[]
                {
                    new MemberValidationException("foobar", new[] { "bar" }),
                    new MemberValidationException("lorem", new[] { "ipsum", "dolor" }),
                };

                /* Act */
                var ex = new ModelValidationException("foo", memberErrors);

                /* Assert */
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    e => Assert.Equal(new[] { "bar" }, e.Value),
                    e => Assert.Equal(new[] { "ipsum", "dolor" }, e.Value)
                );
            }
        }

        public class IEnumerableImpl : CommunicationExceptionTests
        {
            [Fact]
            public void HasMembers_ThatListEnumerated()
            {
                /* Arrange */
                var memberErrors = new[]
                {
                    new MemberValidationException("foobar", new[] { "bar" }),
                    new MemberValidationException("lorem", new[] { "ipsum", "dolor" }),
                };

                /* Act */
                var ex = new ModelValidationException("foomsg", memberErrors);

                /* Assert */
                Assert.Collection(ex,
                    e => Assert.Same(memberErrors.First(), e),
                    e => Assert.Same(memberErrors.Last(), e)
                );
            }
        }

        public class IFormattableImpl : CommunicationExceptionTests
        {
            [Fact]
            public void ToString_MessageReturned()
            {
                /* Act */
                var ex = new ModelValidationException("foomsg",
                    A.CollectionOfDummy<MemberValidationException>(1));

                /* Assert */
                Assert.Equal("foomsg", ex.ToString());
            }
        }
    }
}
