using System.Globalization;
using System.Linq;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// General string helper methods extending the base class library
    /// </summary>
    public static class StringExtensions
    {
        public static string ConvertToPascalCase(this string str)
            => string.Join(".", str.Split('.')
                .Select(s => char.ToUpper(s.ToCharArray()[0], CultureInfo.InvariantCulture) + s[1..]));
    }
}
