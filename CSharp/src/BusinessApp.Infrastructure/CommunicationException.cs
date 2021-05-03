using System;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Exception representing errors with communication with external dependencies
    /// </summary>
    public class CommunicationException : Exception
    {
        public CommunicationException(string message, Exception? inner = null)
            : base(message, inner)
        { }
    }
}
