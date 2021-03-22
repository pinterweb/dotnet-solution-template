namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Hateoas links for <see cre="IDomainEvent" />
    /// </summary>
    public abstract class HateoasEventLink<E> : HateoasLink<IDomainEvent>
        where E : IDomainEvent
    {
        public HateoasEventLink(string rel)
            : base(rel)
        {
            RelativeLinkFactory = (e) => EventRelativeLinkFactory((E)e);
        }

        protected abstract Func<E, string> EventRelativeLinkFactory { get; }
    }
}
