namespace BusinessApp.WebApi
{
    /// <summary>
    /// Registers services to support HATEOAS as defined in RFC8288
    /// </summary>
    public class WeblinkingHeaderRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public WeblinkingHeaderRegister(IBootstrapRegister inner)
        {
            this.inner = inner;
        }

        public void Register(RegistrationContext context)
        {
            inner.Register(context);

            var serviceType = typeof(IHttpRequestHandler<,>);
            var pipeline = context.GetPipelineBuilder(serviceType);

            pipeline.RunOnce(typeof(WeblinkingHeaderRequestDecorator<,>));
        }
    }
}
