namespace BusinessApp.WebApi.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Security;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Microsoft.AspNetCore.Http;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using SimpleInjector;
    using Xunit;

    public class ExceptionExtensionsTests
    {
        private HttpContext http;

        public ExceptionExtensionsTests()
        {
            http = A.Fake<HttpContext>();
        }

        public class OnArgumentException : ExceptionExtensionsTests
        {
            [Fact]
            public void WithoutInnerFormatException_MappedToServerError()
            {
                /* Arrange */
                var ex = new ArgumentException("foo");

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("An unexpected error has occurred in the system.", response.Title);
            }

            [Fact]
            public void WithInnerFormatException_MappedToBadRequest()
            {
                /* Arrange */
                var ex = new ArgumentException("foo", "ba", new FormatException());

                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void WithFormatException_UrlMappedToInvalidRequest()
            {
                /* Arrange */
                var ex = new ArgumentException("foo", "ba", new FormatException());

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/invalid-request", response.Type.AbsolutePath);
            }

            [Fact]
            public void WithFormatException_DetailIsArgumentExceptionMessage()
            {
                /* Arrange */
                var ex = new ArgumentException("foo", "bar", new FormatException());

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foo", response.Detail);
            }

            [Fact]
            public void WithFormatException_TitleInResponse()
            {
                /* Arrange */
                var ex = new ArgumentException("foo", "bar", new FormatException());

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Invalid Request", response.Title);
            }

            [Fact]
            public void WithFormatException_ErrorsMappedFromArgumentExceptionData()
            {
                /* Arrange */
                var format = new FormatException();
                var ex = new ArgumentException("foo", "bar", format);
                ex.Data.Add("bar", "lorem");
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "bar", new[] { "lorem" } },
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnFormatException : ExceptionExtensionsTests
        {
            private FormatException ex;

            public OnFormatException()
            {
                ex = new FormatException("foo");
            }

            [Fact]
            public void StatusCode_MappedToBadResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToInvalidRequest()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/invalid-request", response.Type.AbsolutePath);
            }

            [Theory]
            [InlineData("Byte", "byte")]
            [InlineData("Int16", "short")]
            [InlineData("Int32", "int")]
            [InlineData("Int64", "long")]
            [InlineData("Double", "decimal")]
            [InlineData("Decimal", "decimal")]
            [InlineData("DateTime", "date")]
            [InlineData("Boolean", "bool")]
            public void DetailMessage_InResponse(string dataType, string expectedReplacement)
            {
                /* Arrange */
                ex = new FormatException(dataType);

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expectedReplacement, response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Invalid Request", response.Title);
            }

            [Fact]
            public void Errors_InResponse()
            {
                /* Arrange */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "", new[] { "foo" } }
                };
                ex.Data.Add("", "foo");

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnModelValidationException : ExceptionExtensionsTests
        {
            private readonly ModelValidationException ex;

            public OnModelValidationException()
            {
                ex = new ModelValidationException("foo", A.CollectionOfDummy<MemberValidationException>(1));
            }

            [Fact]
            public void StatusCode_MappedToBadResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToInvalidData()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/invalid-data", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(
                    "Your data was not accepted because it is not valid. Please fix the errors",
                    response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Invalid Data", response.Title);
            }

            [Fact]
            public void Errors_InResponse()
            {
                /* Arrange */
                var ex = new ModelValidationException("foo", new[]
                {
                    new MemberValidationException("bar", new[] { "foo" }),
                    new MemberValidationException("lorem", new[] { "ipsum" }),
                });
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "bar", new[] { "foo" } },
                    { "lorem", new[] { "ipsum" } },
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnBadStateException : ExceptionExtensionsTests
        {
            private readonly BadStateException ex;

            public OnBadStateException()
            {
                ex = new BadStateException("foo");
            }

            [Fact]
            public void StatusCode_MappedToBadResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToInvalidData()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/invalid-data", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(
                    "Your data was not accepted because it is not valid. Please fix the errors",
                    response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Invalid Data", response.Title);
            }

            [Fact]
            public void Errors_InResponse()
            {
                /* Arrange */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "", new[] { "foo" } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnActivationException : ExceptionExtensionsTests
        {
            private readonly ActivationException ex;

            public OnActivationException()
            {
                ex = new ActivationException("foo");
            }

            [Fact]
            public void StatusCode_MappedToNotFoundResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(404)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotFound()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/not-found", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_ExceptionMessageMappedInResponse()
            {
                /* Arrange */
                A.CallTo(() => http.Request.Path).Returns("/foobar");

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("No resource was found at /foobar", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Resource not found", response.Title);
            }

            [Fact]
            public void Errors_NotInResponse()
            {
                /* Arrange */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnEntityNotFoundException : ExceptionExtensionsTests
        {
            private readonly EntityNotFoundException ex;

            public OnEntityNotFoundException()
            {
                ex = new EntityNotFoundException("foo message");
            }

            [Fact]
            public void StatusCode_MappedToNotFoundResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(404)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotFound()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/not-found", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_ExceptionMessageMappedInResponse()
            {
                /* Arrange */
                A.CallTo(() => http.Request.Path).Returns("/foobar");

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("No resource was found at /foobar", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Resource not found", response.Title);
            }

            [Fact]
            public void Errors_MappedFromExceptionData()
            {
                /*  */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "", new[] { "foo message" } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnSecurityResourceException : ExceptionExtensionsTests
        {
            private readonly SecurityResourceException ex;

            public OnSecurityResourceException()
            {
                ex = new SecurityResourceException("foo", "bar");
            }

            [Fact]
            public void StatusCode_MappedToForbiddenResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(403)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToInsufficientPrivileges()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/insufficient-privileges", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_ExceptionMessageOmitted()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Empty(response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Insufficient privileges", response.Title);
            }

            [Fact]
            public void Errors_ResourceAndMessageInResponse()
            {
                /* Arrange */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "foo", new[] { "bar" } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnSecurityException : ExceptionExtensionsTests
        {
            private readonly SecurityException ex;

            public OnSecurityException()
            {
                ex = new SecurityException("foo");
            }

            [Fact]
            public void StatusCode_MappedToForbiddenResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(403)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToInsufficientPrivileges()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/insufficient-privileges", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_OmittedSinceInErrors()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foo", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Insufficient privileges", response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnDBConcurrencyException : ExceptionExtensionsTests
        {
            private readonly DBConcurrencyException ex;

            public OnDBConcurrencyException()
            {
                ex = new DBConcurrencyException("foo");
            }

            [Fact]
            public void StatusCode_MappedToConflictResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(409)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToConflict()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/conflict", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(
                    "There was a conflict while updating your data. Please try again. " +
                    "If you continue to see this error please contact support.",
                    response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("There was a conflict while updating your data.", response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnNotSupportedException : ExceptionExtensionsTests
        {
            private readonly NotSupportedException ex;

            public OnNotSupportedException()
            {
                ex = new NotSupportedException("foo");
            }

            [Fact]
            public void StatusCode_MappedToNotImplementedResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(501)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotSupported()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/not-supported", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_Omitted()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foo", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("The operation is not supported.", response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnNotImplementedException : ExceptionExtensionsTests
        {
            private readonly NotImplementedException ex;

            public OnNotImplementedException()
            {
                ex = new NotImplementedException("foo");
            }

            [Fact]
            public void StatusCode_MappedToNotImplementedResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(501)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotSupported()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/not-supported", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_Omitted()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Empty(response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("The operation is not supported.", response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnInvalidOperationException : ExceptionExtensionsTests
        {
            private readonly InvalidOperationException ex;

            public OnInvalidOperationException()
            {
                ex = new InvalidOperationException("foo");
            }

            [Fact]
            public void StatusCode_MappedToBadRequestResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotSupported()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/not-supported", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_MappedFromExceptionMessage()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foo", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("The operation is not supported because of bad data.",
                    response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnTaskCanceledException : ExceptionExtensionsTests
        {
            private readonly TaskCanceledException ex;

            public OnTaskCanceledException()
            {
                ex = new TaskCanceledException("foo");
            }

            [Fact]
            public void StatusCode_MappedToBadRequestResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(400)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToCanceled()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/canceled", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_Omitted()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Empty(response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("The request has been canceled.",
                    response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnAggregateException : ExceptionExtensionsTests
        {
            private AggregateException ex;

            public OnAggregateException()
            {
                ex = new AggregateException(new Exception[]
                {
                    new BadStateException("foo"),
                    new InvalidOperationException("lorem")
                });
            }

            [Fact]
            public void StatusCode_MappedFromLastException()
            {
                /* Act */
                ex = new AggregateException(new Exception[]
                {
                    new BadStateException("foo"),
                    new SecurityException("ipsum"),
                });

                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(403)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToNotSupported()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/multiple-errors", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_Omitted()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Empty(response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Multiple Errors Occurred. Please see the errros.",
                    response.Title);
            }

            [Fact]
            public void Errors_MappedFromInnerExceptions()
            {
                /* Arrange */
                ex = new AggregateException(new Exception[]
                {
                    new ModelValidationException("foo", new[]
                    {
                        new MemberValidationException("foo", new[] { "bar" }),
                    }),
                    new InvalidOperationException("lorem"),
                    new TaskCanceledException("lorem")
                });

                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "foo", new[] { "bar" } },
                    { "", new[] { "lorem", "The request has been canceled." } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class OnException : ExceptionExtensionsTests
        {
            private Exception ex;

            public OnException()
            {
                ex = new Exception();
                ex.Data.Add("foo", "bar");
            }

            [Fact]
            public void StatusCode_MappedToInternalServerError()
            {
                /* Arrange */
                A.CallTo(() => http.Response.StatusCode).Returns(200);

                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(500)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToUnknown()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/unexpected", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_MappedFromString()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
#if DEBUG
                Assert.Equal("Exception of type 'System.Exception' was thrown.",
                    response.Detail);
#else
                Assert.Equal("An unexpected error has occurred in the system. " +
                    "The problem has been logged. Please contact support if this " +
                    "issue persists without our acknowledgment.",
                    response.Detail);
#endif
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("An unexpected error has occurred in the system.",
                    response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Null(response.Errors);
            }
        }

        public class OnCommunicationException : ExceptionExtensionsTests
        {
            private readonly CommunicationException ex;

            public OnCommunicationException()
            {
                ex = new CommunicationException("foo");
            }

            [Fact]
            public void StatusCode_MappedToFailedDependencyResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(424)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToCommunicationError()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/dependent-error", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_MappedFromExceptionMessage()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foo", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Communication Error",
                    response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Arrange */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "", new[] { "foo" } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }
        }

        public class WithInnerException : ExceptionExtensionsTests
        {
            private readonly FooException ex;

            public WithInnerException()
            {
                ex = new FooException();
                ex.InnerException.Data.Add("", "lorem");
            }

            [Fact]
            public void StatusCode_MappedToFailedDependencyResponse()
            {
                /* Act */
                ex.MapToWebResponse(http);

                /* Assert */
                A.CallToSet(() => http.Response.StatusCode).To(403)
                    .MustHaveHappenedOnceExactly();
            }

            [Fact]
            public void Url_MappedToCommunicationError()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("/docs/errors/insufficient-privileges", response.Type.AbsolutePath);
            }

            [Fact]
            public void DetailMessage_MappedFromExceptionMessage()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("foobar", response.Detail);
            }

            [Fact]
            public void Title_InResponse()
            {
                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal("Insufficient privileges", response.Title);
            }

            [Fact]
            public void Errors_EmptyInResponse()
            {
                /* Arrange */
                var expected = new Dictionary<string, IEnumerable<string>>
                {
                    { "", new[] { "lorem" } }
                };

                /* Act */
                var response = ex.MapToWebResponse(http);

                /* Assert */
                Assert.Equal(expected, response.Errors);
            }

            private class FooException : Exception
            {
                public FooException() : base("foo", new SecurityException("foobar")) {  }
            }
        }
    }
}
