namespace BusinessApp.WebApi
{
    public class PipelineBuilderOptions
    {
        public ScopeBehavior ScopeBehavior { get; set; }
        public RequestType RequestType { get; set; }
        public Lifetime Lifetime { get; set; }
    }
}
