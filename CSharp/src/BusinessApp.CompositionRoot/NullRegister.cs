using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessApp.Infrastructure;
using BusinessApp.Kernel;

namespace BusinessApp.CompositionRoot
{
    /// <summary>
    /// Registers null services that may be removed based on the template install flags
    /// </summary>
    public class NullRegister : IBootstrapRegister
    {
        private readonly IBootstrapRegister inner;

        public NullRegister(IBootstrapRegister inner) => this.inner = inner;

#if DEBUG
        public void Register(RegistrationContext context) => inner.Register(context);
#else
        public void Register(RegistrationContext context)
        {
            inner.Register(context);
            var container = context.Container;

#if events
            container.Register<IEventStoreFactory, NullEventStoreFactory>();
#endif
        }
#endif

#if DEBUG
        private sealed class NullEventStoreFactory : IEventStoreFactory
        {
            public IEventStore Create<T>(T eventTrigger) where T : class => new NullEventStore();

            private sealed class NullEventStore : IEventStore
            {
                public EventTrackingId Add<T>(T e) where T : notnull, IEvent
                    => new(new MetadataId(0), new MetadataId(0));
            }
        }
#elif events
        private sealed class NullEventStoreFactory : IEventStoreFactory
        {
            public IEventStore Create<T>(T eventTrigger) where T : class => new NullEventStore();

            private sealed class NullEventStore : IEventStore
            {
                public EventTrackingId Add<T>(T e) where T : notnull, IEvent
                    => new(new MetadataId(0), new MetadataId(0));
            }
        }
#endif
    }
}
