namespace BusinessApp.WebApi
{
    using System;
    using System.Collections.Generic;

    public interface IPipelineBuilder
    {
        IPipelineBuilder RunOnce(Type type, RequestType? requestType = null);
        IPipelineBuilder Run(Type type, RequestType? requestType = null);
        IEnumerable<(Type, PipelineOptions)>  Build();
        IPipelineIntegration Integrate(Type type);
        IPipelineIntegration IntegrateOnce(Type type);
    }
}

