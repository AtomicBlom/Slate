using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static BinaryVibrance.INPCSourceGenerator.SyntaxTreeHelpers;

namespace BinaryVibrance.INPCSourceGenerator
{
    [Flags]
    public enum PropertyAccess
    {
        Public = 0,
        GetterPrivate = 1,
        GetterWriteonly = 2,
        SetterPrivate = 4,
        SetterReadonly = 8
    }
    
    [Generator]
    public class NotifyPropertyChangedSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization(i =>
            {
                string attributeSource = $@"
#nullable enable
using System;
using System.CodeDom.Compiler;

namespace {typeof(NotifyPropertyChangedSourceGenerator).Namespace}
{{
    [{MakeGeneratedAttribute().ToFullString()}]
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class ImplementNotifyPropertyChangedAttribute : Attribute
    {{
        public ImplementNotifyPropertyChangedAttribute() {{ }}
        public ImplementNotifyPropertyChangedAttribute(PropertyAccess access) 
        {{ 
            Access = access;
        }}
        public string? PropertyName {{ get; set; }}
        public Type? ExposedType {{ get; set; }}
        public PropertyAccess Access {{ get; set; }}
    }}

    [{MakeGeneratedAttribute().ToFullString()}]
    [Flags]
    internal enum PropertyAccess {{
        GetterPrivate = 1,
        GetterWriteonly = 2,
        SetterPrivate = 4,
        SetterReadonly = 8
    }}
}}
";

                string resharperAttributeSource = $@"
#nullable enable
using System;
using System.CodeDom.Compiler;

namespace {typeof(NotifyPropertyChangedSourceGenerator).Namespace}
{{
    [{MakeGeneratedAttribute().ToFullString()}]
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {{
        public NotifyPropertyChangedInvocatorAttribute() {{ }}
        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {{
            ParameterName = parameterName;
        }}

        public string? ParameterName {{ get; }}
    }}
}}
";
                i.AddSource("BinaryVibrance.ImplementNotifyPropertyChangedAttribute.cs", attributeSource);
                i.AddSource("BinaryVibrance.ResharperNotifyPropertyChangedInvocatorAttribute.cs", resharperAttributeSource);
            });

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new NotifyPropertyChangedSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is NotifyPropertyChangedSyntaxReceiver receiver))
                return;

            var attributeName =
                $"{typeof(NotifyPropertyChangedSourceGenerator).Namespace}.ImplementNotifyPropertyChangedAttribute";
            // get the added attribute, and INotifyPropertyChanged
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(attributeName)
                                               ?? throw new Exception($"Could not find {attributeName}");
            INamedTypeSymbol notifySymbol =
                context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged")
                ?? throw new Exception("Could not find System.ComponentModel.INotifyPropertyChanged");

            // group the fields by class, and generate the source
            foreach (var group in receiver.Fields.GroupBy<IFieldSymbol, INamedTypeSymbol>(f => f.ContainingType,
                SymbolEqualityComparer.Default))
            {
                var classSource = ProcessClass(group.Key, group.ToList(), attributeSymbol, notifySymbol, context);
                if (classSource is null) continue;

                context.AddSource($"{group.Key.Name}_autoNotify.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string? ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol attributeSymbol,
            ISymbol notifySymbol, GeneratorExecutionContext context)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
                return null; //TODO: issue a diagnostic that it must be top level

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            
            var classMembers = new[]
            {
                NotifyClassCodeGeneration.GeneratePropertyChangedEvent(),
                NotifyClassCodeGeneration.GenerateRaisePropertyChanged(),
                NotifyClassCodeGeneration.GenerateSetFieldMethod()
            }.Concat(fields
                .Select(fieldSymbol => NotifyClassCodeGeneration.ProcessField(fieldSymbol, attributeSymbol, context))
                .Where(m => m is not null)
                .Cast<MemberDeclarationSyntax>()
            );

            var file = CompilationUnit()
                .WithUsings(
                    MakeUsingList(
                        Using("System.ComponentModel")
                            .WithLeadingTrivia(TriviaList(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)))),
                        Using("System.CodeDom.Compiler"),
                        Using("System.Collections.Generic"),
                        Using("System.Runtime.CompilerServices")))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(namespaceName))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(classSymbol.Name)
                                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                                        .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(notifySymbol.Name)))))
                                        .WithMembers(List(classMembers))))));

            //source.Append("} }");
            return file.NormalizeWhitespace().ToFullString();
            //return source.ToString();
        }
    }

    public class NotifyClassCodeGeneration
    {
        internal static MemberDeclarationSyntax GenerateSetFieldMethod()
        {
            var genericParameterTName = IdentifierName("T");
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("SetField"))
                .WithAttributeLists(MakeAttributeList(MakeGeneratedAttribute()))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                .WithTypeParameterList(TypeParameterList(SingletonSeparatedList(TypeParameter(Identifier("T")))))
                .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier("field"))
                                .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                                .WithType(genericParameterTName),
                            Parameter(Identifier("value"))
                                .WithType(genericParameterTName),
                            Parameter(Identifier("propertyName"))
                                .WithAttributeLists(MakeAttributeList(Attribute(IdentifierName("CallerMemberName"))))
                                .WithType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                                .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                            ))
                .WithBody(
                    Block(
                        IfStatement(
                            InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            GenericName(Identifier("EqualityComparer"))
                                                .WithTypeArgumentList(TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(genericParameterTName))),
                                            IdentifierName("Default")),
                                        IdentifierName("Equals")))
                                .WithArgumentList(
                                    MakeArgumentList(
                                        Argument(IdentifierName("field")),
                                        Argument(IdentifierName("value"))
                                        )),
                            ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression))),
                        ExpressionStatement(
                            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("field"), IdentifierName("value"))),
                        ExpressionStatement(
                            InvocationExpression(IdentifierName("RaisePropertyChanged"))
                                .WithArgumentList(MakeArgumentList(
                                    Argument(IdentifierName("propertyName"))
                                    ))),
                        ReturnStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression))));
        }

        internal static MemberDeclarationSyntax GenerateRaisePropertyChanged()
        {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("RaisePropertyChanged"))
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword)))
                .WithParameterList(
                    MakeParameterList(
                        Parameter(Identifier("propertyName"))
                            .WithType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        ConditionalAccessExpression(
                            IdentifierName("PropertyChanged"),
                            InvocationExpression(
                                    MemberBindingExpression(
                                        IdentifierName("Invoke")))
                                .WithArgumentList(
                                    MakeArgumentList(
                                        Argument(ThisExpression()),
                                        Argument(
                                            ObjectCreationExpression(IdentifierName("PropertyChangedEventArgs"))
                                                .WithArgumentList(
                                                    MakeArgumentList(Argument(IdentifierName("propertyName"))))))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        internal static MemberDeclarationSyntax GeneratePropertyChangedEvent()
        {
            return EventFieldDeclaration(
                    VariableDeclaration(NullableType(IdentifierName("PropertyChangedEventHandler")))
                        .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("PropertyChanged")))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
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
                var getterAccessorDeclarationSyntax = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithExpressionBody(ArrowExpressionClause(IdentifierName(fieldName)))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                if ((access & PropertyAccess.GetterPrivate) == PropertyAccess.GetterPrivate)
                {
                    getterAccessorDeclarationSyntax = getterAccessorDeclarationSyntax.WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PrivateKeyword)));
                }
                accessors.Add(getterAccessorDeclarationSyntax);
            }

            if (canSet)
            {
                var setterAccessorDeclarationSyntax = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithExpressionBody(ArrowExpressionClause(
                        InvocationExpression(IdentifierName("SetField"))
                            .WithArgumentList(
                                MakeArgumentList(
                                    Argument(IdentifierName(fieldName))
                                        .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                                    Argument(IdentifierName("value"))
                                )
                            )
                    ))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                if ((access & PropertyAccess.SetterPrivate) == PropertyAccess.SetterPrivate)
                {
                    setterAccessorDeclarationSyntax = setterAccessorDeclarationSyntax.WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PrivateKeyword)));
                }

                accessors.Add(setterAccessorDeclarationSyntax);
            }

            var property = PropertyDeclaration(ParseTypeName(fieldType.ToString()), propertyName)
                .WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(MakeGeneratedAttribute())
                        )))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(AccessorList(new SyntaxList<AccessorDeclarationSyntax>(accessors)));

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
            if (context.Node is not FieldDeclarationSyntax
            {
                AttributeLists: { Count: > 0 }
            } fieldDeclarationSyntax) return;
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