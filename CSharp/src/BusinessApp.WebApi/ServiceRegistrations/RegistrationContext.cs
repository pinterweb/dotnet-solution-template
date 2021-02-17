namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    public class RegistrationContext
    {
        private readonly IDictionary<Type, IPipelineBuilder> pipelines;

        public RegistrationContext()
        {
            pipelines = new Dictionary<Type, IPipelineBuilder>();
        }

        public Container Container { get; set; }

        public IPipelineBuilder GetPipeline(Type serviceType)
        {
            if (pipelines.TryGetValue(serviceType, out IPipelineBuilder pipeline))
            {
                return pipeline;
            }

            pipeline = new ScopedPipeline(serviceType);
            pipelines.Add(serviceType, pipeline);

            return pipeline;
        }
    }
}
