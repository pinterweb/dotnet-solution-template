namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using BusinessApp.Domain;

    /// <summary>
    /// Data structure for returning an exception to the client
    /// ref: https://tools.ietf.org/html/rfc7807#section-4.1
    /// </summary>
    public class ResponseProblemBody
    {
        public ResponseProblemBody(int status, string title, Uri type)
        {
            Detail = Title = GuardAgainst.Empty(title, nameof(title));
            Type = GuardAgainst.Null(type, nameof(type));
            Status = status;
            Errors = new Dictionary<string, IEnumerable<string>>();
        }

        public int Status { get; private set; }
        public string Title { get; private set; }
        public string Detail { get; set; }
        public IDictionary<string, IEnumerable<string>> Errors { get; set; }
        public Uri Type { get; private set; }
    }
}
