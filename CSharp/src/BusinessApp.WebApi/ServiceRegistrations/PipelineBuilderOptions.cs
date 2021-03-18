namespace BusinessApp.WebApi
{
    using System;

    public record PipelineBuilderOptions
    {
        public ScopeBehavior ScopeBehavior { get; set; }
        public RequestType RequestType { get; set; }
        public Lifetime Lifetime { get; set; }
        public Func<Type, bool> ServiceFilter { get; set; }
    }
}
