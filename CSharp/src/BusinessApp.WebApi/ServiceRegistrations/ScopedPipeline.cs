namespace BusinessApp.WebApi
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class ScopedPipeline : IPipelineBuilder
    {
        private readonly List<(Type, PipelineOptions)> decorators;
        private readonly IDictionary<Type, IntegrationTarget> integrations;
        private readonly List<int> scopeIndexes;
        private readonly Type serviceType;

        public ScopedPipeline(Type serviceType)
        {
            integrations = new Dictionary<Type, IntegrationTarget>();
            decorators = new List<(Type, PipelineOptions)>();
            scopeIndexes = new List<int>();
            this.serviceType = serviceType;
        }

        public IPipelineBuilder RunOnce(Type type, RequestType? requestType = null)
        {
            return Run(type, new PipelineOptions
            {
                ScopeBehavior = scopeIndexes.Any() ? ScopeBehavior.Inner : ScopeBehavior.Outer,
                RequestType = requestType ?? RequestType.Default
            });
        }

        public IPipelineBuilder Run(Type type, RequestType? requestType = null)
        {
            return Run(type, new PipelineOptions
            {
                RequestType = requestType ?? RequestType.Default
            });
        }

        public IPipelineIntegration IntegrateOnce(Type type)
        {
            return new PipelineIntegration(this, type, new PipelineOptions());
        }

        public IPipelineIntegration Integrate(Type type)
        {
            return new PipelineIntegration(this, type, new PipelineOptions());
        }

        public IEnumerable<(Type, PipelineOptions)> Build()
        {
            foreach(var i in integrations)
            {
                var targetIndex = decorators.FindIndex(t => t.Item1 == i.Value.Type);

                if (targetIndex == -1)
                {
                    throw new BusinessAppWebApiException($"Cannot integration {i.Key.Name} " +
                        $"because ${i.Value.Type.Name} is not in the ${serviceType.Name} pipeline");
                }

                decorators.Insert(targetIndex + i.Value.Offset, (i.Key, i.Value.Options));
            }

            var finalDecorators = new List<(Type, PipelineOptions)>(decorators);
            decorators.Clear();

            return finalDecorators.ToArray();
        }

        private IPipelineBuilder Run(Type type, PipelineOptions options)
        {
            if (decorators.Find(l => l.Item1 == type) != default)
            {
                throw new BusinessAppWebApiException($"{type.Name} is already registered");
            }

            var isDecorator = type.GetConstructors()
                .FirstOrDefault()
                ?.GetParameters()
                ?.Any(p =>
                    serviceType == p.ParameterType ||
                    (
                        p.ParameterType.IsGenericType&&
                        serviceType == p.ParameterType.GetGenericTypeDefinition()
                    ))
                ?? false;

            if (!isDecorator)
            {
                scopeIndexes.Add(decorators.Count);
            }

            decorators.Add((type, options));

            return this;
        }

        private class PipelineIntegration : IPipelineIntegration
        {
            private readonly Type integrationType;
            private readonly ScopedPipeline pipeline;
            private readonly PipelineOptions options;

            public PipelineIntegration(ScopedPipeline pipeline,
                Type integrationType,
                PipelineOptions options)
            {
                this.pipeline = pipeline;
                this.integrationType = integrationType;
                this.options = options;
            }

            public IPipelineBuilder After(Type type)
            {
                pipeline.integrations.Add(integrationType, new IntegrationTarget
                {
                    Type = type,
                    Offset = 1,
                    Options = options,
                });

                return pipeline;
            }

            public IPipelineBuilder Before(Type type)
            {
                pipeline.integrations.Add(integrationType, new IntegrationTarget
                {
                    Type = type,
                    Offset = -1,
                    Options = options,
                });

                return pipeline;
            }
        }

        private class IntegrationTarget
        {
            public Type Type { get; set; }
            public int Offset { get; set; }
            public PipelineOptions Options { get; set; }
        }
    }
}
