using System.Globalization;
using System.Linq;

namespace BusinessApp.Infrastructure
{
    public static class StringExtensions
    {
        public static string ConvertToPascalCase(this string str)
            => string.Join(".", str.Split('.')
                .Select(s => char.ToUpper(s.ToCharArray()[0], CultureInfo.InvariantCulture) + s[1..]));
    }
}
