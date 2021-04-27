using Microsoft.CodeAnalysis;

namespace BusinessApp.Analyzers
{
    public static class ISymbolExtensions
    {
        public static bool IsString(this ITypeSymbol symbol)
            => symbol.ToString().StartsWith("string");
    }
}
