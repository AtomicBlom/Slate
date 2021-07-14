using System;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace BinaryVibrance.MLEM.Binding.Generator
{
    public static class SyntaxTreeHelpers2
    {
        public static SyntaxList<AttributeListSyntax> MakeAttributeList(params AttributeSyntax[] attributes)
            => MakeSeparatedList(attributes, a => SingletonList(AttributeList(a)));

        public static AttributeArgumentListSyntax MakeAttributeArgumentList(params AttributeArgumentSyntax[] attributes)
            => MakeSeparatedList(attributes, AttributeArgumentList);

        public static ArgumentListSyntax MakeArgumentList(params ArgumentSyntax[] arguments)
            => MakeSeparatedList(arguments, ArgumentList);

        public static TypeArgumentListSyntax MakeTypeArgumentList(params TypeSyntax[] arguments) 
            => MakeSeparatedList(arguments, TypeArgumentList);

        public static TypeParameterListSyntax MakeTypeParameterList(params TypeParameterSyntax[] typeParameters)
            => MakeSeparatedList(typeParameters, TypeParameterList);

        public static ParameterListSyntax MakeParameterList(params ParameterSyntax[] parameters)
            => MakeSeparatedList(parameters, ParameterList);

        public static SeparatedSyntaxList<TypeParameterConstraintSyntax> MakeTypeParameterConstraintList(params TypeParameterConstraintSyntax[] parameters)
            => MakeSeparatedList(parameters, a => a);

        private static TOutput MakeSeparatedList<TInput, TOutput>(TInput[] arguments, Func<SeparatedSyntaxList<TInput>, TOutput> buildList) where TInput : SyntaxNode
        {
            if (arguments.Length == 1)
            {
                return buildList(SingletonSeparatedList(arguments.Single()));
            }

            return buildList(
                SeparatedList<TInput>(
                    arguments
                        .SelectMany(x => new SyntaxNodeOrToken[] { x, Token(SyntaxKind.CommaToken) })
                        .Take(arguments.Length * 2 - 1)
                        .ToArray()
                )
            );
        }

        public static SyntaxList<TypeParameterConstraintClauseSyntax> MakeTypeConstraintParameterList(params TypeParameterConstraintClauseSyntax[] typeParameters)
            => typeParameters.Length == 1 ? SingletonList(typeParameters.Single()) : List(typeParameters);

        public static SyntaxList<UsingDirectiveSyntax> MakeUsingList(params UsingDirectiveSyntax[] parameters) 
            => parameters.Length == 1 ? SingletonList(parameters.Single()) : List(parameters);

        public static UsingDirectiveSyntax Using(string ns)
        {
            var parts = ns.Split('.');
            return UsingDirective(
                parts.Skip(1).Aggregate((NameSyntax)IdentifierName(parts[0]), (left, right) => QualifiedName(left, IdentifierName(right)))
            );
        }

        public static AttributeSyntax MakeGeneratedAttribute<T>()
        {
            var name = typeof(T).Assembly.GetName();
            var assemblyName = name.Name;
            var assemblyVersion = name.Version.ToString();

            return Attribute(IdentifierName(nameof(GeneratedCodeAttribute)))
                .WithArgumentList(
                    MakeAttributeArgumentList(
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(assemblyName))),
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(assemblyVersion)))
                    )
                );
        }
    }
}