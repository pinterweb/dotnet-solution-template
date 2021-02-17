namespace BusinessApp.WebApi
{
    using SimpleInjector;

    public enum Lifetime
    {
        Scope,
        Singleton
    }

    public static class SimpleInjectorLifetimeExtensions
    {
        public static Lifestyle MapLifestyle(this Lifetime lifetime)
        {
            return lifetime switch
            {
                Lifetime.Singleton => Lifestyle.Singleton,
                _ => Lifestyle.Scoped
            };
        }
    }
}

