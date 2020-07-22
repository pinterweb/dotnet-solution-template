using System.ComponentModel.DataAnnotations;

namespace BusinessApp.App.UnitTest
{
    using System;
    using Xunit;
    using System.Collections;
    using FakeItEasy;
    using System.Linq;

    public class ValidationExceptionTests
    {
        public class Constructor : ValidationExceptionTests
        {
            [Fact]
            public void WithResultAndInnerExcpetion_ResultMappedToResultProp()
            {
                /* Arrange */
                var result = new ValidationResult("foo");

                /* Act */
                var ex = new ValidationException(result, A.Dummy<Exception>());

                /* Assert */
                Assert.Same(result, ex.Result);
            }

            [Fact]
            public void WithResultAndInnerExcpetion_ResultMappedToData()
            {
                /* Arrange */
                var result = new ValidationResult("foo", new[] { "bar", "lorem" });

                /* Act */
                var ex = new ValidationException(result, A.Dummy<Exception>());

                /* Assert */
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    d =>
                    {
                        Assert.Equal("bar", d.Key);
                        Assert.Equal("foo", d.Value);
                    },
                    d =>
                    {
                        Assert.Equal("lorem", d.Key);
                        Assert.Equal("foo", d.Value);
                    });
            }

            [Fact]
            public void WithResultAndInnerException_InnerMappedToProp()
            {
                /* Arrange */
                var inner = new Exception();

                /* Act */
                var ex = new ValidationException(A.Dummy<ValidationResult>(), inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }

            [Fact]
            public void WithMemberMessageAndInner_MemberNameAndMessageInResult()
            {
                /* Arrange */
                var member = "foo";
                var message = "bar";

                /* Act */
                var ex = new ValidationException(member, message);

                /* Assert */
                Assert.Equal(message, ex.Result.ErrorMessage);
                Assert.Contains(member, ex.Result.MemberNames);
            }

            [Fact]
            public void WithMemberMessageAndInner_InnerMappedToProp()
            {
                /* Arrange */
                var inner = new Exception();

                /* Act */
                var ex = new ValidationException(A.Dummy<string>(), A.Dummy<string>(), inner);

                /* Assert */
                Assert.Same(inner, ex.InnerException);
            }

            [Fact]
            public void WithMemberMessageAndInner_MemberNameAndMessageInData()
            {
                /* Arrange */
                var member = "foo";
                var message = "bar";

                /* Act */
                var ex = new ValidationException(member, message);

                /* Assert */
                Assert.Equal(message, ex.Result.ErrorMessage);
                Assert.Contains(member, ex.Result.MemberNames);
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    d =>
                    {
                        Assert.Equal("foo", d.Key);
                        Assert.Equal("bar", d.Value);
                    });
            }

            [Fact]
            public void WithMessage_MessageInResult()
            {
                /* Arrange */
                var message = "bar";

                /* Act */
                var ex = new ValidationException(message);

                /* Assert */
                Assert.Equal(message, ex.Result.ErrorMessage);
            }

            [Fact]
            public void WithMessage_MessageInData()
            {
                /* Arrange */
                var message = "bar";

                /* Act */
                var ex = new ValidationException(message);

                /* Assert */
                Assert.Equal(message, ex.Result.ErrorMessage);
                Assert.Collection(ex.Data.Cast<DictionaryEntry>(),
                    d =>
                    {
                        Assert.Equal("", d.Key);
                        Assert.Equal("bar", d.Value);
                    });
            }
        }
    }
}
