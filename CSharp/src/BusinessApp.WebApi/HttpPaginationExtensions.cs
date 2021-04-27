using BusinessApp.Infrastructure;

namespace BusinessApp.WebApi
{
    public static class HttpPaginationExtensions
    {
        public static string[] ToHeaderValue(this Pagination page) => new[]
        {
            $"count={page.ItemCount}"
        };
    }

}
