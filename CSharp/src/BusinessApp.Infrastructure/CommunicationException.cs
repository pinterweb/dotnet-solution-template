using System;

namespace BusinessApp.Infrastructure
{
    public class CommunicationException : Exception
    {
        public CommunicationException(string message, Exception? inner = null)
            : base(message, inner)
        { }
    }
}
