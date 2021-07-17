using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryVibrance.MLEM.Binding.Generator
{
    public class BindingSyntaxReceiver : ISyntaxContextReceiver
    {
        public HashSet<INamedTypeSymbol> Classes { get; } = new(SymbolEqualityComparer.Default);
        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is InvocationExpressionSyntax ies &&
                ies.Expression is MemberAccessExpressionSyntax maes &&
                maes.Name.ToFullString() == "Bind" &&
                context.SemanticModel.GetSymbolInfo(ies.Expression).Symbol is IMethodSymbol bindMethodSymbol &&
                bindMethodSymbol.ContainingType.Name == "ElementBindingExtensions")
            {
                var namedTypeSymbol = (INamedTypeSymbol)bindMethodSymbol.TypeArguments[1];
                if (!Classes.Contains(namedTypeSymbol))
                {
                    Classes.Add(namedTypeSymbol);
                }
            }
        }
    }
}