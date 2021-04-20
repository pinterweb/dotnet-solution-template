using System;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Data structure to support HATEOAS
    /// </summary>
    /// <typeparam name="T">The request type</typeparam>
    /// <typeparam name="R">The response type</typeparam>
    public class HateoasLink<T, R>
    {
        protected HateoasLink(string rel)
        {
            Rel = rel.NotEmpty().Expect(rel);
            RelativeLinkFactory = (t, r) => rel;
        }

        public HateoasLink(Func<T, R, string> relativeLinkFactory, string rel)
            : this(rel)
        {
            RelativeLinkFactory = relativeLinkFactory.NotNull().Expect(nameof(relativeLinkFactory));
        }

        public virtual Func<T, R, string> RelativeLinkFactory { get; }
        public string Rel { get; }
        public string? Title { get; init; }
    }
}
