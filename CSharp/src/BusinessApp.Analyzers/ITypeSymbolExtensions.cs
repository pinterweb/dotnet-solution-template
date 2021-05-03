using Microsoft.CodeAnalysis;

namespace BusinessApp.Analyzers
{
    /// <summary>
    /// Extensions for <see cref="ITypeSymbol" />
    /// </summary>
    public static class ITypeSymbolExtensions
    {
        public static bool IsString(this ITypeSymbol symbol)
            => symbol.ToString().StartsWith("string");
    }
}
