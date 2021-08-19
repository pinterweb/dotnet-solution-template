using System;
using System.Runtime.Serialization;

namespace BusinessApp.Infrastructure.Json
{
    /// <summary>
    /// Custom exception thrown while deserializing json data
    /// </summary>
    [Serializable]
    public class JsonDeserializationException : Exception
    {
        public JsonDeserializationException(string? message, Exception? innerException) : base(message, innerException)
        { }

        protected JsonDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
