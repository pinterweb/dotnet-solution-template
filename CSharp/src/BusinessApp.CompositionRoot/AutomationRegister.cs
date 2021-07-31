using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using Microsoft.Extensions.Options;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers automation services
    /// </summary>
    public class AutomationRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;
        private readonly RegistrationOptions options;

        public AutomationRegister(IBootstrapRegister inner, RegistrationOptions options)
        {
            this.inner = inner;
            this.options = options;
        }

        public void Register(RegistrationContext context)
        {
            var container = context.Container;
            var serviceType = typeof(IRequestHandler<,>);

            container.Register(typeof(IRequestMapper<,>), options.RegistrationAssemblies);
            container.Register<IProcessManager, SimpleInjectorProcessManager>();

            inner.Register(context);

#if DEBUG
            container.Register<IRequestStore>(() => container.GetInstance<Infrastructure.Persistence.BusinessAppDbContext>());
#elif efcore
            container.Register<IRequestStore>(() => container.GetInstance<Infrastructure.Persistence.BusinessAppDbContext>());
#else
            container.Register<IRequestStore, NullRequestStore>();
#endif

            container.RegisterDecorator(
                serviceType,
                typeof(AutomationRequestDecorator<,>),
                c => c.AppliedDecorators
                    .Where(a => a.IsGenericType)
                    .Any(a => a.GetGenericTypeDefinition() == typeof(EventConsumingRequestDecorator<,>)));
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
