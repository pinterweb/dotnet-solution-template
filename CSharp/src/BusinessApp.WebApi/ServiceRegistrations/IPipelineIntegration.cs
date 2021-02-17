namespace BusinessApp.WebApi
{
    using System;

    public interface IPipelineIntegration
    {
        IPipelineBuilder Before(Type type);
        IPipelineBuilder After(Type type);
    }
}

