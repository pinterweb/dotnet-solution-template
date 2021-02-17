namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;
    using SimpleInjector;

    public class RegistrationContext
    {
        private readonly IDictionary<Type, IPipelineBuilder> builders;

        public RegistrationContext()
        {
            builders = new Dictionary<Type, IPipelineBuilder>();
        }

        public Container Container { get; set; }

        public IPipelineBuilder GetPipelineBuilder(Type serviceType)
        {
            if (builders.TryGetValue(serviceType, out IPipelineBuilder builder))
            {
                return builder;
            }

            builder = new ScopedPipeline(serviceType);
            builders.Add(serviceType, builder);

            return builder;
        }
    }
}
