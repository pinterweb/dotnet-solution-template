namespace BusinessApp.WebApi.IntegrationTest
{
    using SimpleInjector;
    using System.Collections.Generic;

    public static class SimpleInjectorTestExtensions
    {
        public static IEnumerable<InstanceProducer> GetDependencies(this InstanceProducer p)
        {
            foreach (var r in p.GetRelationships())
            {
                yield return r.Dependency;
                foreach (var d in r.Dependency.GetDependencies())
                {
                    yield return d;
                }
            }
        }

    }
}
