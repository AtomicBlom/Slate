using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryVibrance.INPCSourceGenerator
{
    public static class SyntaxTreeHelpers
    {
        public static SyntaxList<AttributeListSyntax> MakeAttributeList(params AttributeSyntax[] attributes)
        {
            if (attributes.Length == 1)
            {
                return SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attributes.Single())));
            }

            return SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList<AttributeSyntax>(
                    attributes
                        .SelectMany(x => new SyntaxNodeOrToken[] { x, SyntaxFactory.Token(SyntaxKind.CommaToken) })
                        .Take(attributes.Length * 2 - 1)
                        .ToArray()
                ))
            );
        }

        public static ArgumentListSyntax MakeArgumentList(params ArgumentSyntax[] arguments)
        {
            if (arguments.Length == 1)
            {
                return SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(arguments.Single()));
            }

            return SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                    arguments
                        .SelectMany(x => new SyntaxNodeOrToken[] { x, SyntaxFactory.Token(SyntaxKind.CommaToken) })
                        .Take(arguments.Length * 2 - 1)
                        .ToArray()
                )
            );
        }

        public static ParameterListSyntax MakeParameterList(params ParameterSyntax[] parameters)
        {
            if (parameters.Length == 1)
            {
                return SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(parameters.Single()));
            }

            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList<ParameterSyntax>(
                    parameters
                        .SelectMany(x => new SyntaxNodeOrToken[] { x, SyntaxFactory.Token(SyntaxKind.CommaToken) })
                        .Take(parameters.Length * 2 - 1)
                        .ToArray()
                )
            );
        }

        public static SyntaxList<UsingDirectiveSyntax> MakeUsingList(params UsingDirectiveSyntax[] parameters)
        {
            return parameters.Length == 1 ? SyntaxFactory.SingletonList(parameters.Single()) : SyntaxFactory.List(parameters);
        }

        public static UsingDirectiveSyntax Using(string ns)
        {
            var parts = ns.Split('.');
            return SyntaxFactory.UsingDirective(
                parts.Skip(1).Aggregate((NameSyntax)SyntaxFactory.IdentifierName(parts[0]), (left, right) => SyntaxFactory.QualifiedName(left, SyntaxFactory.IdentifierName(right)))
            );
        }

        public static AttributeSyntax MakeGeneratedAttribute()
        {
            var name = typeof(NotifyPropertyChangedSourceGenerator).Assembly.GetName();
            var assemblyName = name.Name;
            var assemblyVersion = name.Version.ToString();

            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(nameof(GeneratedCodeAttribute)))
                .WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(assemblyName))),
                                SyntaxFactory.Token(SyntaxKind.CommaToken),
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(assemblyVersion)))
                            }
                        )
                    )
                );
        }
    }
}