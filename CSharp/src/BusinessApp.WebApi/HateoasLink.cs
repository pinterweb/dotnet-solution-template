namespace BusinessApp.WebApi
{
    using System;
    using BusinessApp.Domain;

    /// <summary>
    /// Data structure to support HATEOAS
    /// </summary>
    public class HateoasLink<R>
    {
        public HateoasLink(Func<R, string> relativeLinkFactory, string rel, string title)
        {
            RelativeLinkFactory = relativeLinkFactory.NotNull().Expect(nameof(relativeLinkFactory));
            Rel = rel.NotNull().Expect(rel);
            Title = title;
        }

        public Func<R, string> RelativeLinkFactory { get; }
        public string Rel { get; }
        public string Title { get; }
    }
}
