using System;
using BusinessApp.Domain;

namespace BusinessApp.Data
{
    /// <summary>
    /// Custom exception to throw when an error occurrs during data operations
    /// </summary>
    [Serializable]
    public class BusinessAppDataException : BusinessAppException
    {
        public BusinessAppDataException(string message, Exception? inner = null)
            :base(message, inner)
        { }
    }
}
