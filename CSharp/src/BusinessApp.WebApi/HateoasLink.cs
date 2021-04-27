using System;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Data structure to support HATEOAS
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class HateoasLink<TRequest, TResponse>
    {
        protected HateoasLink(string rel)
        {
            Rel = rel.NotEmpty().Expect(rel);
            RelativeLinkFactory = (t, r) => rel;
        }

        public HateoasLink(Func<TRequest, TResponse, string> relativeLinkFactory, string rel)
            : this(rel)
                => RelativeLinkFactory = relativeLinkFactory.NotNull().Expect(nameof(relativeLinkFactory));

        public virtual Func<TRequest, TResponse, string> RelativeLinkFactory { get; }
        public string Rel { get; }
        public string? Title { get; init; }
    }
}
