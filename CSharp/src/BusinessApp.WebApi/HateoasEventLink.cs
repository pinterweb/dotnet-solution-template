using System;
using BusinessApp.Domain;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Hateoas links for <see cre="IDomainEvent" />
    /// </summary>
    /// <typeparam name="T">The request type that triggered the event</typeparam>
    /// <typeparam name="E">The event type</typeparam>
    public abstract class HateoasEventLink<T, E> : HateoasLink<T, IDomainEvent>
        where E : IDomainEvent
    {
        public HateoasEventLink(string rel)
            : base(rel)
        { }

        public override Func<T, IDomainEvent, string> RelativeLinkFactory
            => (r, e) => EventRelativeLinkFactory(r, (E)e);
        protected abstract Func<T, E, string> EventRelativeLinkFactory { get; }
    }
}
