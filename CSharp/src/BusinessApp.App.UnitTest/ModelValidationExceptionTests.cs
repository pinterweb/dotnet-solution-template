namespace BusinessApp.App.UnitTest
{
    using Xunit;
    using System.Collections.Generic;
    using FakeItEasy;
    using BusinessApp.App;
    using System.Linq;
    using BusinessApp.Domain;

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
                Assert.IsType<BusinessAppException>(ex);
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
        }

        public class IEnumerableImpl : ModelValidationExceptionTests
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
    }
}
