namespace BusinessApp.Infrastructure.WebApi
{
    /// <summary>
    /// Paging extensions for <see cref="Pagination" />
    /// </summary>
    public static class HttpPaginationExtensions
    {
        public static string[] ToHeaderValue(this Pagination page) => new[]
        {
            $"count={page.ItemCount}"
        };
    }
}
