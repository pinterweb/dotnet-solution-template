using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers null services that may be removed based on the template install flags
    /// </summary>
    public class RequestMetadataRegister : IBootstrapRegister
    {
        private readonly RegistrationOptions options;
        private readonly IBootstrapRegister inner;

        public RequestMetadataRegister(RegistrationOptions options, IBootstrapRegister inner)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;
            container.Register(typeof(IRequestMapper<,>), options.RegistrationAssemblies);

            inner.Register(context);

#if DEBUG
            container.Register<IRequestStore>(() => container.GetInstance<Infrastructure.Persistence.BusinessAppDbContext>());
#else
#if efcore
            container.Register<IRequestStore>(() => container.GetInstance<Infrastructure.Persistence.BusinessAppDbContext>());
#else
            container.Register<IRequestStore, NullRequestStore>();
#endif
#endif
        }

#if !efcore
        private sealed class NullRequestStore : IRequestStore
        {
            public Task<IEnumerable<RequestMetadata>> GetAllAsync()
                => Task.FromResult<IEnumerable<RequestMetadata>>(Array.Empty<RequestMetadata>());
        }
#endif
    }
}
