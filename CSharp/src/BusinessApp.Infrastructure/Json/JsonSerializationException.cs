using System;
using System.Runtime.Serialization;

namespace BusinessApp.Infrastructure.Json
{
    /// <summary>
    /// Custom exception thrown while serializing an object to json
    /// </summary>
    [Serializable]
    public class JsonSerializationException : Exception
    {
        public JsonSerializationException(string? message, Exception? innerException) : base(message, innerException)
        { }

        protected JsonSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
