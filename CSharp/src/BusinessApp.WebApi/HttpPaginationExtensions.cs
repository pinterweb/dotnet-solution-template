namespace BusinessApp.WebApi
{
    using BusinessApp.App;

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
