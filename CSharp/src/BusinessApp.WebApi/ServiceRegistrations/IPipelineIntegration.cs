namespace BusinessApp.WebApi
{
    using System;

    public interface IPipelineIntegration
    {
        IPipeline Before(Type type);
        IPipeline After(Type type);
    }
}

