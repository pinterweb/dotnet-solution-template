namespace BusinessApp.WebApi
{
    /// <summary>
    /// Registers services to support HATEOAS as defined in rfc5988
    /// </summary>
    public class WeblinkingRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public WeblinkingRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            inner.Register(context);

            var serviceType = typeof(IHttpRequestHandler<,>);
            var pipeline = context.GetPipelineBuilder(serviceType);

            pipeline.RunOnce(typeof(WeblinkingRequestDecorator<,>));
        }
    }
}
