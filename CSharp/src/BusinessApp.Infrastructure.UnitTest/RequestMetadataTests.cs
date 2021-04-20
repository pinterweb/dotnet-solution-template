using System;
using System.Collections.Generic;
using BusinessApp.Domain;
using FakeItEasy;
using Xunit;

namespace BusinessApp.Infrastructure.UnitTest
{
    public class RequestMetadataTests
    {
        public class Constructor : RequestMetadataTests
        {
            public static IEnumerable<object[]> InvalidStringCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    typeof(RequestMetadata).FullName,
                    "requestType: Cannot be missing or the type '' cannot be created from a string"
                },
                new object[]
                {
                    "",
                    typeof(RequestMetadata).FullName,
                    "requestType: Cannot be missing or the type '' cannot be created from a string"
                },
                new object[]
                {
                    typeof(RequestMetadata).Name,
                    typeof(RequestMetadata).FullName,
                    "requestType: Cannot be missing or the type 'RequestMetadata' cannot be created from a string"
                },
                new object[]
                {
                    typeof(RequestMetadata).FullName,
                    null,
                    "responseType: Cannot be missing or the type '' cannot be created from a string"
                },
                new object[]
                {
                    typeof(RequestMetadata).FullName,
                    "",
                    "responseType: Cannot be missing or the type '' cannot be created from a string"
                },
                new object[]
                {
                    typeof(RequestMetadata).FullName,
                    typeof(RequestMetadata).Name,
                    "responseType: Cannot be missing or the type 'RequestMetadata' cannot be created from a string"
                }
            };

            public static IEnumerable<object[]> InvalidTypeCtorArgs => new[]
            {
                new object[]
                {
                    null,
                    A.Dummy<Type>(),
                    "requestType: Value cannot be null"
                },
                new object[]
                {
                    A.Dummy<Type>(),
                    null,
                    "responseType: Value cannot be null"
                },
            };

            [Theory, MemberData(nameof(InvalidStringCtorArgs))]
            public void InvalidStringCtorArgs_ExceptionThrown(string requestType, string responseType,
                string errMsg)
            {
                /* Arrange */
                void shouldThrow() => new RequestMetadata(requestType, responseType);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(errMsg, ex.Message);
            }

            [Theory, MemberData(nameof(InvalidTypeCtorArgs))]
            public void InvalidTypeCtorArgs_ExceptionThrown(Type requestType, Type responseType,
                string errMsg)
            {
                /* Arrange */
                void shouldThrow() => new RequestMetadata(requestType, responseType);

                /* Act */
                var ex = Record.Exception(shouldThrow);

                /* Assert */
                Assert.IsType<BusinessAppException>(ex);
                Assert.Equal(errMsg, ex.Message);
            }
        }

        public class EqualsOverride : RequestMetadataTests
        {
            [Fact]
            public void NotRequestMetadata_NotEqual()
            {
                /* Arrange */
                var sut = new RequestMetadata(typeof(int), typeof(string));

                /* Act */
                var areEqual = sut.Equals(A.Dummy<object>());

                /* Assert */
                Assert.False(areEqual);
            }

            [Fact]
            public void OtherIsNull_NotEqual()
            {
                /* Arrange */
                var sut = new RequestMetadata(typeof(int), typeof(string));

                /* Act */
                var areEqual = sut.Equals(null);

                /* Assert */
                Assert.False(areEqual);
            }

            [Theory]
            [InlineData(typeof(string), typeof(int), false)]
            [InlineData(typeof(int), typeof(string), true)]
            public void RequestAndReponseTypeEqual_TrueRetrned(Type a, Type b, bool expectTrue)
            {
                /* Arrange */
                var sut = new RequestMetadata(typeof(int), typeof(string));
                var other = new RequestMetadata(a, b);

                /* Act */
                var areEqual = sut.Equals(other);

                /* Assert */
                Assert.Equal(expectTrue, areEqual);
            }
        }
    }
}
