namespace BusinessApp.WebApi
{
    public class PipelineOptions
    {
        public ScopeBehavior ScopeBehavior { get; set; }
        public RequestType RequestType { get; set; }
        public Lifetime Lifetime { get; set; }
    }
}
