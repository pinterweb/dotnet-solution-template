using BusinessApp.Infrastructure;

namespace BusinessApp.Infrastructure.WebApi
{
    public static class HttpPaginationExtensions
    {
        public static string[] ToHeaderValue(this Pagination page) => new[]
        {
            $"count={page.ItemCount}"
        };
    }

}
