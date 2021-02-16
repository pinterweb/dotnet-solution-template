namespace BusinessApp.WebApi
{
    using System;

    public class PipelineOptions
    {
        public ScopeBehavior ScopeBehavior { get; set; }
        public RequestType RequestType { get; set; }
        public Lifetime Lifetime { get; set; }
        public Func<Type, Type, bool> Test { get; set; }
    }
}

