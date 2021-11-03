using System.Collections.Generic;

namespace BusinessApp.WebApi.IntegrationTest
{
    /// <summary>
    /// Used for test deserialization
    /// </summary>
    public class CompositeProblemDetailStub
    {
        public int StatusCode { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
        public IEnumerable<ProblemDetailStub> Responses { get; set; }
    }
}
