namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Data structure to support HATEOAS
    /// </summary>
    public class HateoasLink<R>
    {
        protected HateoasLink(string rel)
        {
            Rel = rel.NotEmpty().Expect(rel);
        }

        public HateoasLink(Func<R, string> relativeLinkFactory, string rel)
            : this(rel)
        {
            RelativeLinkFactory = relativeLinkFactory.NotNull().Expect(nameof(relativeLinkFactory));
        }

        public Func<R, string> RelativeLinkFactory { get; protected set; }
        public string Rel { get; }
        public string Title { get; init; }
    }
}
