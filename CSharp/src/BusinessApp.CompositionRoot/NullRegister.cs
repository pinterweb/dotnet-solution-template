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

            container.RegisterConditional<IEventStoreFactory, NullEventStoreFactory>(c => !c.Handled);
            container.RegisterConditional<ITransactionFactory, NullTransactionFactory>(c => !c.Handled);
        }
#endif

        private sealed class NullEventStoreFactory : IEventStoreFactory
        {
            public IEventStore Create<T>(T eventTrigger) where T : class => new NullEventStore();

            private sealed class NullEventStore : IEventStore
            {
                public EventTrackingId Add<T>(T e) where T : notnull, IDomainEvent
                    => new(new MetadataId(0), new MetadataId(0));
            }
        }

        private sealed class NullTransactionFactory : ITransactionFactory
        {
            private static readonly IUnitOfWork nullUow = new NullUnitOfWork();

            public Result<IUnitOfWork, Exception> Begin() => Result.Ok(nullUow);

            private sealed class NullUnitOfWork : IUnitOfWork
            {
                public event EventHandler Committing = delegate { };
                public event EventHandler Committed = delegate { };

                public void Add<T>(T aggregate) where T : AggregateRoot { }
                public Task CommitAsync(CancellationToken cancelToken) => Task.CompletedTask;
                public T? Find<T>(Func<T, bool> filter) where T : AggregateRoot => null;
                public void Remove<T>(T aggregate) where T : AggregateRoot { }
                public Task RevertAsync(CancellationToken cancelToken) => Task.CompletedTask;
                public void Track<T>(T aggregate) where T : AggregateRoot { }
            }
        }
    }
}
