using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryVibrance.INPCSourceGenerator
{
    public class NotifyClassCodeGeneration
    {
        internal static MemberDeclarationSyntax GenerateSetFieldMethod()
        {
            var genericParameterTName = SyntaxFactory.IdentifierName("T");
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), SyntaxFactory.Identifier("SetField"))
                .WithAttributeLists(SyntaxTreeHelpers.MakeAttributeList(SyntaxTreeHelpers.MakeGeneratedAttribute()))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                .WithTypeParameterList(SyntaxFactory.TypeParameterList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.TypeParameter(SyntaxFactory.Identifier("T")))))
                .WithParameterList(
                    SyntaxTreeHelpers.MakeParameterList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("field"))
                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)))
                            .WithType(genericParameterTName),
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                            .WithType(genericParameterTName),
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("propertyName"))
                            .WithAttributeLists(SyntaxTreeHelpers.MakeAttributeList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("CallerMemberName"))))
                            .WithType(SyntaxFactory.NullableType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))))
                            .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    ))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.GenericName(SyntaxFactory.Identifier("EqualityComparer"))
                                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(genericParameterTName))),
                                            SyntaxFactory.IdentifierName("Default")),
                                        SyntaxFactory.IdentifierName("Equals")))
                                .WithArgumentList(
                                    SyntaxTreeHelpers.MakeArgumentList(
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("field")),
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                                    )),
                            SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("field"), SyntaxFactory.IdentifierName("value"))),
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("RaisePropertyChanged"))
                                .WithArgumentList(SyntaxTreeHelpers.MakeArgumentList(
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("propertyName"))
                                ))),
                        SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))));
        }

        internal static MemberDeclarationSyntax GenerateRaisePropertyChanged()
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), SyntaxFactory.Identifier("RaisePropertyChanged"))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)))
                .WithParameterList(
                    SyntaxTreeHelpers.MakeParameterList(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("propertyName"))
                            .WithType(SyntaxFactory.NullableType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.ConditionalAccessExpression(
                            SyntaxFactory.IdentifierName("PropertyChanged"),
                            SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberBindingExpression(
                                        SyntaxFactory.IdentifierName("Invoke")))
                                .WithArgumentList(
                                    SyntaxTreeHelpers.MakeArgumentList(
                                        SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("PropertyChangedEventArgs"))
                                                .WithArgumentList(
                                                    SyntaxTreeHelpers.MakeArgumentList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("propertyName"))))))))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        internal static MemberDeclarationSyntax GeneratePropertyChangedEvent()
        {
            return SyntaxFactory.EventFieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.NullableType(SyntaxFactory.IdentifierName("PropertyChangedEventHandler")))
                        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("PropertyChanged")))))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
        }

        internal static MemberDeclarationSyntax? ProcessField(IFieldSymbol fieldSymbol, ISymbol attributeSymbol, GeneratorExecutionContext context)
        {
            // get the name and type of the field
            string fieldName = fieldSymbol.Name;
            ITypeSymbol fieldType = fieldSymbol.Type;

            // get the AutoNotify attribute from the field, and any associated data
            AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad =>
                ad.AttributeClass!.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            var overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PropertyName").Value;
            var exposedTypeOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "ExposedType").Value;
            var accessOpt = 
                attributeData.ConstructorArguments.Any() 
                    ? attributeData.ConstructorArguments[0] 
                    : attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "Access").Value;
            var accessIsSpecified = attributeData.ConstructorArguments.Any() ||
                                    attributeData.NamedArguments.Any(kvp => kvp.Key == "Access");


            var accessVal = accessOpt.Value as int? ?? 0;
            var access = (PropertyAccess)accessVal;

            var canGet = (access & PropertyAccess.GetterWriteonly) != PropertyAccess.GetterWriteonly;
            var canSet = (access & PropertyAccess.SetterReadonly) != PropertyAccess.SetterReadonly;

            string propertyName = ChoosePropertyName(fieldName, overridenNameOpt);
            if (propertyName.Length == 0 || propertyName == fieldName)
                //TODO: issue a diagnostic that we can't process this field
                return null;

            if (fieldSymbol.IsReadOnly)
            {
                canSet = false;
            }

            if (!exposedTypeOpt.IsNull)
            {
                if (accessIsSpecified && canSet)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor("BVINPC001", "Invalid use of PropertyAccess", "PropertyAccess.{} cannot be used in conjunection with ExposedType", "BinaryVibrance.ViewModel", DiagnosticSeverity.Error, true),
                            fieldSymbol.Locations.FirstOrDefault(),
                            access.ToString()
                        )
                    );
                }
                
                //TODO: Make sure that the field is convertable to this type.
                fieldType = (INamedTypeSymbol)exposedTypeOpt.Value!;
                canSet = false;
            }

            var accessors = new List<AccessorDeclarationSyntax>();
            if (canGet)
            {
                var getterAccessorDeclarationSyntax = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(fieldName)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                if ((access & PropertyAccess.GetterPrivate) == PropertyAccess.GetterPrivate)
                {
                    getterAccessorDeclarationSyntax = getterAccessorDeclarationSyntax.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }
                accessors.Add(getterAccessorDeclarationSyntax);
            }

            if (canSet)
            {
                var setterAccessorDeclarationSyntax = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("SetField"))
                            .WithArgumentList(
                                SyntaxTreeHelpers.MakeArgumentList(
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldName))
                                        .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.RefKeyword)),
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                                )
                            )
                    ))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                if ((access & PropertyAccess.SetterPrivate) == PropertyAccess.SetterPrivate)
                {
                    setterAccessorDeclarationSyntax = setterAccessorDeclarationSyntax.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));
                }

                accessors.Add(setterAccessorDeclarationSyntax);
            }

            var property = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(fieldType.ToString()), propertyName)
                .WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxTreeHelpers.MakeGeneratedAttribute())
                        )))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxFactory.AccessorList(new SyntaxList<AccessorDeclarationSyntax>(accessors)));

            return property;
        }

        private static string ChoosePropertyName(string fieldName, TypedConstant overridenNameOpt)
        {
            if (!overridenNameOpt.IsNull) return overridenNameOpt.Value!.ToString();

            fieldName = fieldName.TrimStart('_');
            if (fieldName.Length == 0)
                return string.Empty;

            if (fieldName.Length == 1)
                return fieldName.ToUpper();

            return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
        }
    }
}