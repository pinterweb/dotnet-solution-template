namespace BusinessApp.App
{
    using System;

    [Serializable]
    public class CommunicationException : Exception
    {
        public CommunicationException(string message, Exception? inner = null)
            :base(message, inner)
        {
            Data.Add("", message);
        }
    }
}
