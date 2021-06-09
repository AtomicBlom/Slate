using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
namespace {typeof(NotifyPropertyChangedSourceGenerator).Namespace}
{{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ImplementNotifyPropertyChangedAttribute : Attribute
    {{
        public ImplementNotifyPropertyChangedAttribute() {{ }}
        public string? PropertyName {{ get; set; }}
    }}
}}
";

                string resharperAttributeSource = $@"
#nullable enable
using System;
namespace {typeof(NotifyPropertyChangedSourceGenerator).Namespace}
{{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
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

            var attributeName = $"{typeof(NotifyPropertyChangedSourceGenerator).Namespace}.ImplementNotifyPropertyChangedAttribute";
            // get the added attribute, and INotifyPropertyChanged
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName(attributeName);
            INamedTypeSymbol notifySymbol = context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");

            // group the fields by class, and generate the source
            foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy(f => f.ContainingType))
            {
                string classSource = ProcessClass(group.Key, group.ToList(), attributeSymbol, notifySymbol, context);
                context.AddSource($"{group.Key.Name}_autoNotify.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol attributeSymbol, ISymbol notifySymbol, GeneratorExecutionContext context)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null; //TODO: issue a diagnostic that it must be top level
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // begin building the generated source
            StringBuilder source = new StringBuilder($@"
#nullable enable
using System.ComponentModel;
using {typeof(NotifyPropertyChangedSourceGenerator).Namespace};

namespace {namespaceName}
{{
    public partial class {classSymbol.Name} : {notifySymbol.ToDisplayString()}
    {{
");

            // if the class doesn't implement INotifyPropertyChanged already, add it
            if (!classSymbol.Interfaces.Contains(notifySymbol))
            {
                source.Append($@"
public event PropertyChangedEventHandler? PropertyChanged;
        
protected void RaisePropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

[NotifyPropertyChangedInvocator]
protected bool SetField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
{{
    if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
    field = value;
    RaisePropertyChanged(propertyName);
    return true;
}}
                ");

            }

            // create properties for each field 
            foreach (IFieldSymbol fieldSymbol in fields)
            {
                ProcessField(source, fieldSymbol, attributeSymbol);
            }

            source.Append("} }");
            return source.ToString();
        }

        private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol)
        {
            // get the name and type of the field
            string fieldName = fieldSymbol.Name;
            ITypeSymbol fieldType = fieldSymbol.Type;

            // get the AutoNotify attribute from the field, and any associated data
            AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            TypedConstant overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PropertyName").Value;

            string propertyName = chooseName(fieldName, overridenNameOpt);
            if (propertyName.Length == 0 || propertyName == fieldName)
            {
                //TODO: issue a diagnostic that we can't process this field
                return;
            }

            source.Append($@"
public {fieldType} {propertyName} 
{{
    get => {fieldName};
    set => SetField(ref {fieldName}, value);
}}
");

            string chooseName(string fieldName, TypedConstant overridenNameOpt)
            {
                if (!overridenNameOpt.IsNull)
                {
                    return overridenNameOpt.Value.ToString();
                }

                fieldName = fieldName.TrimStart('_');
                if (fieldName.Length == 0)
                    return string.Empty;

                if (fieldName.Length == 1)
                    return fieldName.ToUpper();

                return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
            }

        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class NotifyPropertyChangedSyntaxReceiver : ISyntaxContextReceiver
        {
            public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                var attributeName = $"{typeof(NotifyPropertyChangedSourceGenerator).Namespace}.ImplementNotifyPropertyChangedAttribute";

                // any field with at least one attribute is a candidate for property generation
                if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
                    && fieldDeclarationSyntax.AttributeLists.Count > 0)
                {
                    foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                    {
                        // Get the symbol being declared by the field, and keep it if its annotated
                        IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                        if (fieldSymbol.GetAttributes().Any(ad =>
                        {
                            return ad.AttributeClass.ToDisplayString() == attributeName;
                        }))
                        {
                            Fields.Add(fieldSymbol);
                        }
                    }
                }
            }
        }
    }

}