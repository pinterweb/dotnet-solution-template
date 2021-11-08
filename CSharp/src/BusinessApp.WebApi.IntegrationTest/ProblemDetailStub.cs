namespace BusinessApp.WebApi.IntegrationTest
{
    /// <summary>
    /// Used for test deserialization
    /// </summary>
    public class ProblemDetailStub
    {
        public int StatusCode { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
        public object Data { get; set; }
    }
}
