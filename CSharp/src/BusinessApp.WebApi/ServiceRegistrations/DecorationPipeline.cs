namespace BusinessApp.WebApi
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class DecorationPipeline : IPipelineBuilder
    {
        private readonly List<(Type, PipelineBuilderOptions)> decorators;
        private readonly IDictionary<Type, IntegrationTarget> integrations;
        private readonly List<int> scopeIndexes;
        private readonly Type serviceType;

        public DecorationPipeline(Type serviceType)
        {
            integrations = new Dictionary<Type, IntegrationTarget>();
            decorators = new List<(Type, PipelineBuilderOptions)>();
            scopeIndexes = new List<int>();
            this.serviceType = serviceType;
        }

        public IPipelineBuilder RunOnce(Type type, RequestType? requestType = null)
        {
            return Run(type, new PipelineBuilderOptions
            {
                ScopeBehavior = scopeIndexes.Any() ? ScopeBehavior.Inner : ScopeBehavior.Outer,
                RequestType = requestType ?? RequestType.Default
            });
        }

        public IPipelineBuilder Run(Type type, RequestType? requestType = null)
        {
            return Run(type, new PipelineBuilderOptions
            {
                RequestType = requestType ?? RequestType.Default
            });
        }

        public IPipelineBuilder Run(Type type, PipelineBuilderOptions options)
        {
            return RunCore(type, options);
        }

        public IPipelineIntegration Integrate(Type type)
        {
            return new PipelineIntegration(this, type, new PipelineBuilderOptions());
        }

        public IEnumerable<(Type, PipelineBuilderOptions)> Build()
        {
            foreach(var i in integrations)
            {
                var targetIndex = decorators.FindIndex(t => t.Item1 == i.Value.Type);

                if (targetIndex == -1)
                {
                    throw new BusinessAppWebApiException($"Cannot integration {i.Key.Name} " +
                        $"because ${i.Value.Type.Name} is not in the ${serviceType.Name} pipeline");
                }

                var integrationOptions = decorators[targetIndex].Item2 with
                {
                    ServiceFilter = i.Value.Options.ServiceFilter
                };

                decorators.Insert(targetIndex + i.Value.Offset,
                    (i.Key, integrationOptions));
            }

            var finalDecorators = new List<(Type, PipelineBuilderOptions)>(decorators);
            decorators.Clear();

            return finalDecorators.ToArray();
        }

        private IPipelineBuilder RunCore(Type type, PipelineBuilderOptions options)
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
                        p.ParameterType.IsGenericType
                        && serviceType == p.ParameterType.GetGenericTypeDefinition()
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
            private readonly DecorationPipeline pipeline;
            private readonly PipelineBuilderOptions options;

            public PipelineIntegration(DecorationPipeline pipeline,
                Type integrationType,
                PipelineBuilderOptions options)
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
            public PipelineBuilderOptions Options { get; set; }
        }
    }
}
