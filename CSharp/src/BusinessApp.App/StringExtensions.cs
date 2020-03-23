namespace BusinessApp.App
{
    public static class StringExtensions
    {
        public static string CreateIndexName(this string s, int index)
        {
            return $"[{index}].{s}";
        }
    }
}
