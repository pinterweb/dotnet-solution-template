namespace BusinessApp.Analyzers
{
    using Microsoft.CodeAnalysis;

    public static class ISymbolExtensions
    {
        public static bool IsString(this ITypeSymbol symbol)
        {
            return symbol.ToString().StartsWith("string");
        }
    }
}
