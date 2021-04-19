using Microsoft.CodeAnalysis;

namespace BusinessApp.Analyzers
{
    public static class ISymbolExtensions
    {
        public static bool IsString(this ITypeSymbol symbol)
        {
            return symbol.ToString().StartsWith("string");
        }
    }
}
