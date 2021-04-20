using System;
using System.Collections.Generic;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public class RequestMetadata
    {
        private const string ErrTemplate =
            "Cannot be missing or the type '{0}' cannot be created from a string";

        public RequestMetadata(string requestType, string responseType)
        {
            RequestType = requestType.NotEmpty()
                .Map(r => Type.GetType(r))
                .AndThen(t => t.NotNull())
                .MapError(_ => string.Format(ErrTemplate, requestType))
                .Expect(nameof(requestType))!;

            ResponseType = responseType.NotEmpty()
                .Map(r => Type.GetType(r))
                .AndThen(t => t.NotNull())
                .MapError(_ => string.Format(ErrTemplate, responseType))
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

        public override bool Equals(object? unknown)
        {
            if (unknown is RequestMetadata other)
            {
                return RequestType.Equals(other.RequestType)
                    && ResponseType.Equals(other.ResponseType);
            }

            return base.Equals(unknown);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;
                var hash = (HashingBase * HashingMultiplier) ^ RequestType.GetHashCode();

                return (hash * HashingMultiplier) ^ ResponseType.GetHashCode();
            }
        }
    }
}
