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

                context.AddSource($"{group.Key.Name}_INPC.g.cs", SourceText.From(classSource, Encoding.UTF8));
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
            return file.NormalizeWhitespace().ToFullString();
        }
    }
}