using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryVibrance.MLEM.Binding.Generator
{
    public class BindingSyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IPropertySymbol> Properties { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            var attributeName = typeof(BindAttribute).FullName;

            if (context.Node is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclaration)
            {
                var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration)
                    ?? throw new Exception("Could not find the class being visited.");
                var matchesAttribute = classSymbol
                    .GetAttributes()
                    .Any(ad => ad.AttributeClass?.ToDisplayString() == attributeName);

                if (matchesAttribute)
                {
                    foreach (var member in classDeclaration.Members)
                    {
                        if (context.SemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                            continue;
                        Properties.Add(propertySymbol);
                    }
                }
            }

            if (context.Node is PropertyDeclarationSyntax { AttributeLists: { Count: > 0 } } propertyDeclaration)
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration) as IPropertySymbol
                                     ?? throw new Exception("Could not find the property being visited.");

                var matchesAttribute = propertySymbol
                    .GetAttributes()
                    .Any(ad => ad.AttributeClass?.ToDisplayString() == attributeName);
                if (matchesAttribute)
                {
                    Properties.Add(propertySymbol);
                }
            }
        }
    }
}