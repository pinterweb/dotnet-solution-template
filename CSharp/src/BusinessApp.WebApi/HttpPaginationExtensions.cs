using BusinessApp.App;

namespace BusinessApp.WebApi
{
    public  static class HttpPaginationExtensions
    {
        public static string[] ToHeaderValue(this Pagination page)
        {
            return new[]
            {
                $"count={page.ItemCount}"
            };
        }
    }

}
