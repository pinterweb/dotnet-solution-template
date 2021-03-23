namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Hateoas links for <see cre="IDomainEvent" />
    /// </summary>
    /// <typeparam name="T">The request type that triggered the event</typeparam>
    /// <typeparam name="R">The event type</typeparam>
    public abstract class HateoasEventLink<T, E> : HateoasLink<T, IDomainEvent>
        where E : IDomainEvent
    {
        public HateoasEventLink(string rel)
            : base(rel)
        {
            RelativeLinkFactory = (r, e) => EventRelativeLinkFactory(r, (E)e);
        }

        protected abstract Func<T, E, string> EventRelativeLinkFactory { get; }
    }
}
