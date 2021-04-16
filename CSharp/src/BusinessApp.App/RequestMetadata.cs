namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    public class RequestMetadata
    {
        public RequestMetadata(string requestType, string responseType)
        {
            RequestType = Type.GetType(requestType).Expect($"{requestType} cannot be found");
            ResponseType = Type.GetType(responseType).Expect($"{responseType} cannot be found");
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
