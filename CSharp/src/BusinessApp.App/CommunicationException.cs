using System;

namespace BusinessApp.App
{
    [Serializable]
    public class CommunicationException : Exception
    {
        public CommunicationException(string message, Exception? inner = null)
            :base(message, inner)
        { }
    }
}
