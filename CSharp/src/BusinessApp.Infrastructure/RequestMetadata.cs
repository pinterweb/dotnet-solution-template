using System;
using System.Collections.Generic;
using System.Globalization;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Describes a request/response workflow
    /// </summary>
    public class RequestMetadata
    {
        private const string ErrTemplate =
            "Cannot be missing or the type '{0}' cannot be created from a string";

        public RequestMetadata(string requestType, string responseType)
        {
            RequestType = requestType.NotEmpty()
                .Map(r => Type.GetType(r))
                .AndThen(t => t.NotNull())
                .MapError(_ => string.Format(CultureInfo.InvariantCulture, ErrTemplate, requestType))
                .Expect(nameof(requestType))!;

            ResponseType = responseType.NotEmpty()
                .Map(r => Type.GetType(r))
                .AndThen(t => t.NotNull())
                .MapError(_ => string.Format(CultureInfo.InvariantCulture, ErrTemplate, responseType))
                .Expect(nameof(responseType))!;
        }

        public RequestMetadata(Type requestType, Type responseType)
        {
            RequestType = requestType.NotNull().Expect(nameof(requestType));
            ResponseType = responseType.NotNull().Expect(nameof(responseType));
        }

        public Type RequestType { get; }
        public Type ResponseType { get; }
        public IEnumerable<Type> EventTriggers { get; private set; } = new List<Type>();

        public override bool Equals(object? obj) => obj is RequestMetadata other
            ? RequestType.Equals(other.RequestType) && ResponseType.Equals(other.ResponseType)
            : base.Equals(obj);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                const int multiplier = 23;

                hash = (hash * multiplier) + RequestType.GetHashCode();

                return (hash * multiplier) + ResponseType.GetHashCode();
            }
        }
    }
}
