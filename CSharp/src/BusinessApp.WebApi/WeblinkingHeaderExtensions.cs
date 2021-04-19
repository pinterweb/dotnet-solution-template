using System;
using Microsoft.AspNetCore.Http;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Extensions for the HATEOAS weblinking
    /// </summary>
    public static class WeblinkingHeaderExtensions
    {
        /// <summary>
        /// Converts the <see cref="HateoasLink{T,R}" /> to a header string
        /// </summary>
        public static string ToHeaderValue<T, R>(this HateoasLink<T, R> link, HttpRequest request,
            T requestModel, R responseModel)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var title = string.IsNullOrWhiteSpace(link.Title) ? "" : $";title={link.Title}";
            Func<string> getUrl = () => link.RelativeLinkFactory(requestModel, responseModel);

            return $"<{new Uri(new Uri(baseUrl), getUrl())}>;rel={link.Rel}{title}";

        }
    }
}
