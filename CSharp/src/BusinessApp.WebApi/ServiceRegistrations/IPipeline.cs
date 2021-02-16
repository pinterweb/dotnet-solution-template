namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;

    public interface IPipeline
    {
        IPipeline RunOnce(Type type, RequestType? requestType = null);
        IPipeline RunOnce(Type type, Func<Type, Type, bool> test);
        IPipeline Run(Type type, RequestType? requestType = null);
        IPipeline Run(Type type, Func<Type, Type, bool> test);
        IEnumerable<(Type, PipelineOptions)>  Build();
        IPipelineIntegration Integrate(Type type);
        IPipelineIntegration IntegrateOnce(Type type);
    }
}

