namespace BusinessApp.WebApi
{
    using SimpleInjector;
    using BusinessApp.App;
    using BusinessApp.Data;

    public class QueryRegister: IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;
        private readonly BootstrapOptions options;

        public QueryRegister(IBootstrapRegister inner, BootstrapOptions options)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            RegisterDataServices(context.Container);

            inner.Register(context);
        }

        private void RegisterDataServices(Container container)
        {
            container.Register(typeof(IQueryVisitorFactory<,>),
                typeof(CompositeQueryVisitorBuilder<,>));

            container.Register(typeof(ILinqSpecificationBuilder<,>),
                typeof(AndSpecificationBuilder<,>));

            container.Collection.Register(typeof(ILinqSpecificationBuilder<,>),
                options.RegistrationAssemblies);

            container.Collection.Register(typeof(IQueryVisitor<>), options.RegistrationAssemblies);

            container.RegisterConditional(
                typeof(IQueryVisitor<>),
                typeof(NullQueryVisitor<>), ctx => !ctx.Handled);

            container.Collection.Append(typeof(ILinqSpecificationBuilder<,>), typeof(QueryOperatorSpecificationBuilder<,>));

            container.Collection.Register(typeof(IQueryVisitorFactory<,>), new[]
            {
                typeof(AndSpecificationBuilder<,>),
                typeof(ConstructedQueryVisitorFactory<,>),
            });
        }
    }
}
