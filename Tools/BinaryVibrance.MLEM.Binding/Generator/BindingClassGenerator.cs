using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static BinaryVibrance.MLEM.Binding.Generator.SyntaxTreeHelpers2;

namespace BinaryVibrance.MLEM.Binding.Generator
{
    public class BindingClassGenerator
    {
        private readonly GeneratorExecutionContext _context;

        public BindingClassGenerator(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public CompilationUnitSyntax? GenerateClassSource(INamedTypeSymbol classSymbol, List<IPropertySymbol> properties)
        {
            var members = properties.Select(p => GeneratePropertyExtensionMethod(classSymbol, p)).ToArray();

            var file = CompilationUnit()
                .WithUsings(
                    MakeUsingList(
                        Using("MLEM.Ui.Elements")
                            .WithLeadingTrivia(TriviaList(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)))),
                        Using("BinaryVibrance.MLEM.Binding"))
                    )
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(classSymbol.ContainingNamespace.ToDisplayString()))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration($"{classSymbol.Name}BindingExtensions")
                                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                                        .WithMembers(List(members))
                                )
                            )
                    )
                );
            return file;
        }

        private MemberDeclarationSyntax GeneratePropertyExtensionMethod(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol)
        {
            var propertyBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.PropertyBinding`2")
                                  ?? throw new Exception("Could not locate PropertyBinding<,> in workspace, it should have been added by adding this analyzer.");
            var viewModelBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.ViewModelBinding`2") 
                                   ?? throw new Exception("Could not locate ViewModelBinding<,> in workspace, it should have been added by adding this analyzer.");
            var element = _context.Compilation.GetTypeByMetadataName("MLEM.Ui.Elements.Element")
                ?? throw new Exception("Could not locate MLEM.Ui.Elements.Element, is Mlem.Ui referenced?");
            var tElementGenericTypeIdentifier = IdentifierName("TElement");
            return MethodDeclaration(
                GenericName(propertyBinding.Name)
                    .WithTypeArgumentList(
                        MakeTypeArgumentList(
                            IdentifierName(propertySymbol.Type.ToDisplayString()),
            tElementGenericTypeIdentifier
                            )), 
                Identifier(propertySymbol.Name)
                )
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithTypeParameterList(
                        MakeTypeParameterList(TypeParameter(Identifier("TElement")))
                        )
                    .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier("viewModelBinding"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(viewModelBinding.Name)
                                        .WithTypeArgumentList(
                                            MakeTypeArgumentList(
                                                tElementGenericTypeIdentifier,
                                                IdentifierName(classSymbol.Name)
                                            ))
                                    )
                            )
                        )
                    .WithConstraintClauses(
                        MakeTypeConstraintParameterList(
                            TypeParameterConstraintClause(tElementGenericTypeIdentifier)
                                .WithConstraints(
                                    MakeTypeParameterConstraintList(
                                        TypeConstraint(IdentifierName(element.Name))
                                        )
                                    )
                            ))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ReturnStatement(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token(SyntaxKind.DefaultKeyword))))))

                ;
        }
    }
}