namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Custom exception to throw when an error occurrs during data operations
    /// </summary>
    [Serializable]
    public class BusinessAppWebApiException : BusinessAppException
    {
        public BusinessAppWebApiException(string message)
            :base(message)
        { }
    }
}
