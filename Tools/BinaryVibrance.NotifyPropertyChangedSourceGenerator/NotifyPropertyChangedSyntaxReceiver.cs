using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryVibrance.INPCSourceGenerator
{
    /// <summary>
    ///     Created on demand before each generation pass
    /// </summary>
    internal class NotifyPropertyChangedSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IFieldSymbol> Fields { get; } = new();

        /// <summary>
        ///     Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for
        ///     generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var attributeName =
                $"{typeof(NotifyPropertyChangedSourceGenerator).Namespace}.ImplementNotifyPropertyChangedAttribute";

            // any field with at least one attribute is a candidate for property generation
            if (context.Node is not FieldDeclarationSyntax { AttributeLists: { Count: > 0 } } fieldDeclarationSyntax) return;
            foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
            {
                // Get the symbol being declared by the field, and keep it if its annotated
                if (ModelExtensions.GetDeclaredSymbol(context.SemanticModel, variable) is not IFieldSymbol fieldSymbol)
                    continue;
                if (!fieldSymbol.GetAttributes()
                    .Any(ad => ad.AttributeClass?.ToDisplayString() == attributeName)) continue;

                Fields.Add(fieldSymbol);
            }
        }
    }
}