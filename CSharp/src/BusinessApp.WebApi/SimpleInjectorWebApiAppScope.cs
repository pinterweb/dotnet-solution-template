namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.App;
    using BusinessApp.Domain;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class SimpleInjectorWebApiAppScope : IAppScope
    {
        private readonly Container container;

        public SimpleInjectorWebApiAppScope(Container container)
        {
            this.container = Guard.Against.Null(container).Expect(nameof(container));
        }

        public IDisposable NewScope() => AsyncScopedLifestyle.BeginScope(container);
    }
}
