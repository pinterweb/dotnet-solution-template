namespace BusinessApp.Analyzers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Generator]
    public class EqualsGenerator : ISourceGenerator
    {
        private const string TargetAttributeName = "BusinessApp.Domain.IdAttribute";

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            context.RegisterForSyntaxNotifications(() => new EqualsSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is EqualsSyntaxReceiver s)) return;

            var targetSymbol = context.Compilation.GetTypeByMetadataName(TargetAttributeName);

            foreach (var node in s.TargetSyntaxNodes)
            {
                var model = context.Compilation.GetSemanticModel(node.SyntaxTree);
                var type = model.GetDeclaredSymbol(node, context.CancellationToken) as ITypeSymbol;
                var fullNamespace = type.ContainingNamespace.ToDisplayString();

                var sb = new StringBuilder();
                var propertyEqualities = new List<string>();
                var propertyHashes = new List<string>();

                foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
                {
                    var propertyName = property.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var typeName = property.Type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
                            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
                    var attribute = property.GetAttributes().FirstOrDefault(
                        x => x.AttributeClass?.Equals(targetSymbol, SymbolEqualityComparer.Default) == true);

                    if (attribute != null)
                    {
                        if (property.Type.IsString())
                        {
                            propertyEqualities.Add("string.Equals(" +
                                $"{propertyName}, other.{propertyName}, " +
                                "global::System.StringComparison.OrdinalIgnoreCase)");
                        }
                        else
                        {
                            propertyEqualities.Add("global::System.Collections.Generic" +
                                $".EqualityComparer<{typeName}>.Default.Equals({propertyName}!, "
                                + $"other.{propertyName}!)");
                        }

                        if (property.NullableAnnotation == NullableAnnotation.Annotated)
                        {
                            var hash = $"hash = {propertyName} == null ? 0 : hash * 23 + ";

                            if (property.Type.IsString())
                            {
                                propertyHashes.Add($"{hash} global::System.StringComparer"
                                    + $".OrdinalIgnoreCase.GetHashCode({propertyName});");
                            }
                            else
                            {
                                propertyHashes.Add($"{hash} ({propertyName}?.GetHashCode() ?? 0);");
                            }
                        }
                        else
                        {
                            var hash = $"hash = hash * 23 + ";

                            if (property.Type.IsString())
                            {
                                propertyHashes.Add($"{hash} StringComparer."
                                    + $"OrdinalIgnoreCase.GetHashCode({propertyName}));");
                            }
                                else
                            {
                                propertyHashes.Add($"{hash} {propertyName}!.GetHashCode();");
                            }
                        }
                    }
                }

                sb.Append($@"#nullable enable
namespace {fullNamespace}
{{
    public partial class {type.Name}
    {{
        public override bool Equals(object? obj)
        {{
            if (obj is {type.Name} other)
            {{
                return {string.Join(Environment.NewLine + "&&", propertyEqualities)};
            }}

            return base.Equals(obj);
        }}

        public override int GetHashCode()
        {{
            unchecked
            {{
                int hash = 17;
                {string.Join(System.Environment.NewLine, propertyHashes)}

                return hash;
            }}
        }}
    }}
}}
#nullable restore");

                var fileName = $"{EscapeFileName(type!.ToDisplayString())}.gen.cs";
                context.AddSource(fileName, sb.ToString());
            }
        }

        private static string EscapeFileName(string fileName) =>
            new [] {'<', '>', ','}.Aggregate(new StringBuilder(fileName), (s, c) => s.Replace(c, '_')).ToString();

        internal sealed class EqualsSyntaxReceiver : ISyntaxReceiver
        {
            private readonly List<SyntaxNode> targetSyntaxNodes = new List<SyntaxNode>();

            public IReadOnlyList<SyntaxNode> TargetSyntaxNodes => targetSyntaxNodes;

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax c)
                {
                    foreach (var property in c.Members.OfType<PropertyDeclarationSyntax>())
                    {
                        var hasIdAttribute = property.AttributeLists.Any(a =>
                            a.Attributes.Any(aa => aa.Name.ToString() == "Id"));

                        if (hasIdAttribute)
                        {
                            targetSyntaxNodes.Add(syntaxNode);
                        }
                    }
                }
            }
        }
    }
}
