namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    public class RegistrationContext
    {
        private readonly IDictionary<Type, IPipeline> pipelines;

        public RegistrationContext()
        {
            pipelines = new Dictionary<Type, IPipeline>();
        }

        public Container Container { get; set; }

        public IPipeline GetPipeline(Type serviceType)
        {
            if (pipelines.TryGetValue(serviceType, out IPipeline pipeline))
            {
                return pipeline;
            }

            pipeline = new ScopedPipeline(serviceType);
            pipelines.Add(serviceType, pipeline);

            return pipeline;
        }
    }
}
