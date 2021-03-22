namespace BusinessApp.WebApi
{
    using System;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Extensions for the HATEOAS weblinking
    /// </summary>
    public static class WeblinkingHeaderExtensions
    {
        /// <summary>
        /// Converts the <see cref="HateoasLink{R}" /> to a header string
        /// </summary>
        public static string ToHeaderValue<R>(this HateoasLink<R> link, HttpRequest request, R response)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var title = string.IsNullOrWhiteSpace(link.Title) ? "" : $";title={link.Title}";

            return $"<{new Uri(new Uri(baseUrl), link.RelativeLinkFactory(response))}>;rel={link.Rel}{title}";

        }
    }
}
