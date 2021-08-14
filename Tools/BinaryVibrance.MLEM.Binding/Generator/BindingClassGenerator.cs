using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public CompilationUnitSyntax? GenerateClassSource(INamedTypeSymbol classSymbol)
        {
            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>();

            var parentClassName = $"{classSymbol.Name}BindingExtensions";
            var members = properties
                .Where(p => !classSymbol.IsRecord || p.Name != "EqualityContract")
                .SelectMany(p => new[]
                {
                    GeneratePropertyExtensionMethod(classSymbol, p),
                    GeneratePropertyIdentityExtensionMethod(parentClassName, classSymbol, p)
                } )
                .Where(x => x is not null)
                .Cast<MemberDeclarationSyntax>()
                .ToArray();
            
            var file = CompilationUnit()
                .WithUsings(
                    MakeUsingList(
                        Using("Myra.Graphics2D.UI")
                            .WithLeadingTrivia(TriviaList(Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)))),
                        Using("BinaryVibrance.MLEM.Binding"),
                        Using("System"),
                        Using("System.ComponentModel"),
                        Using("System.Windows.Input")
                    ))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(classSymbol.ContainingNamespace.ToDisplayString()))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(parentClassName)
                                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                                        .WithMembers(List(members))
                                )
                            )
                    )
                );
            return file;
        }

        private MemberDeclarationSyntax GeneratePropertyIdentityExtensionMethod(
            string parentName,
            INamedTypeSymbol classSymbol,
            IPropertySymbol propertySymbol)
        {
            var propertyBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.PropertyBinding`2")
                                  ?? throw new Exception("Could not locate PropertyBinding<,> in workspace, it should have been added by adding this analyzer.");
            var viewModelBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.ViewModelBinding`2")
                                   ?? throw new Exception("Could not locate ViewModelBinding<,> in workspace, it should have been added by adding this analyzer.");
            var widget = _context.Compilation.GetTypeByMetadataName("Myra.Graphics2D.UI.Widget")
                ?? throw new Exception("Could not locate Myra.Graphics2D.UI.Widget, is Myra referenced?");
            
            var tElementGenericTypeIdentifier = IdentifierName("TWidget");

            //Create the method definition
            var viewModelBindingParameter = "viewModelBinding";
            var method = MethodDeclaration(
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
                        MakeTypeParameterList(TypeParameter(Identifier("TWidget")))
                        )
                    .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier(viewModelBindingParameter))
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
                                        TypeConstraint(IdentifierName(widget.Name))
                                        )
                                    )
                            ))
                    .WithBody(
                        Block(
                            ReturnStatement(
                                InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(parentName),
                                            IdentifierName(propertySymbol.Name)))
                                    .WithArgumentList(
                                        MakeArgumentList(
                                            Argument(IdentifierName("viewModelBinding")),
                                            Argument(ObjectCreationExpression(
                                                    GenericName(Identifier("IdentityConverter"))
                                                        .WithTypeArgumentList(
                                                            MakeTypeArgumentList(
                                                                IdentifierName(propertySymbol.Type.ToDisplayString())
                                                            )
                                                        ))
                                                .WithArgumentList(
                                                    ArgumentList())
                                            ))
                                        ))
                        ))
                ;



            return method;
        }

        private MemberDeclarationSyntax? GeneratePropertyExtensionMethod(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol)
        {
            var propertyBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.PropertyBinding`2")
                                  ?? throw new Exception("Could not locate PropertyBinding<,> in workspace, it should have been added by adding this analyzer.");
            var viewModelBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.ViewModelBinding`2") 
                                   ?? throw new Exception("Could not locate ViewModelBinding<,> in workspace, it should have been added by adding this analyzer.");
            var converterInterfaceBinding = _context.Compilation.GetTypeByMetadataName("BinaryVibrance.MLEM.Binding.IConverter`2")
                                   ?? throw new Exception("Could not locate IConverter<,> in workspace, it should have been added by adding this analyzer.");
            var widget = _context.Compilation.GetTypeByMetadataName("Myra.Graphics2D.UI.Widget")
                         ?? throw new Exception("Could not locate Myra.Graphics2D.UI.Widget, is Myra referenced?");

            var propertyChangedInterface = _context.Compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)
                          ?? throw new Exception("Could not locate INotifyPropertyChanged, wut?");

            var tElementGenericTypeIdentifier = IdentifierName("TWidget");

            var canNotify = classSymbol.AllInterfaces.Any(t => t.Equals(propertyChangedInterface, SymbolEqualityComparer.Default));

            //Create the method definition
            var viewModelBindingParameter = "viewModelBinding";
            var converterParameter = "converter";

            var method = MethodDeclaration(
                GenericName(propertyBinding.Name)
                    .WithTypeArgumentList(
                        MakeTypeArgumentList(
                            IdentifierName("TOut"),
                            tElementGenericTypeIdentifier
                            )), 
                Identifier(propertySymbol.Name)
                )
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithTypeParameterList(
                        MakeTypeParameterList(
                            TypeParameter(Identifier("TWidget")),
                            TypeParameter(Identifier("TOut"))
                            )
                        )
                    .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier(viewModelBindingParameter))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(
                                    GenericName(viewModelBinding.Name)
                                        .WithTypeArgumentList(
                                            MakeTypeArgumentList(
                                                tElementGenericTypeIdentifier,
                                                IdentifierName(classSymbol.Name)
                                            ))
                                    ),
                                Parameter(Identifier(converterParameter))
                                    .WithType(
                                        GenericName(converterInterfaceBinding.Name)
                                            .WithTypeArgumentList(
                                                MakeTypeArgumentList(
                                                    IdentifierName(propertySymbol.Type.ToDisplayString()),
                                                    IdentifierName("TOut")
                                                    )
                                                )
                                        )
                            )
                        )
                    .WithConstraintClauses(
                        MakeTypeConstraintParameterList(
                            TypeParameterConstraintClause(tElementGenericTypeIdentifier)
                                .WithConstraints(
                                    MakeTypeParameterConstraintList(
                                        TypeConstraint(IdentifierName(widget.Name))
                                        )
                                    )
                            ));

            var body = Block();

            //Decompose viewModelBinding
            body = body.AddStatements(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        DeclarationExpression(
                            IdentifierName("var"),
                            ParenthesizedVariableDesignation(
                                SeparatedList<VariableDesignationSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        SingleVariableDesignation(
                                            Identifier("widget")),
                                        Token(SyntaxKind.CommaToken),
                                        SingleVariableDesignation(
                                            Identifier("viewModel"))
                                    }))),
                        IdentifierName(viewModelBindingParameter))));

            var propertyBindingConstructorArguments = new List<ArgumentSyntax>
            {
                Argument(IdentifierName("widget"))
            };
            
            if (!propertySymbol.IsWriteOnly)
            {
                //Create getter method
                body = body.AddStatements(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                                GenericName(Identifier("Func"))
                                    .WithTypeArgumentList(
                                        MakeTypeArgumentList(
                                            IdentifierName("TOut")
                                            )
                                        ))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier("getViewModel"))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                ParenthesizedLambdaExpression()
                                                    .WithExpressionBody(
                                                        InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName("converter"),
                                                                    IdentifierName("ConvertTo")))
                                                            .WithArgumentList(
                                                                MakeArgumentList(
                                                                    Argument(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("viewModel"),
                                                                            IdentifierName(propertySymbol.Name)))))
                                                                    )
                                                                )))))
                );
                propertyBindingConstructorArguments.Add(Argument(IdentifierName("getViewModel")));
            }

            if (!propertySymbol.IsReadOnly && propertySymbol.SetMethod is not null && !propertySymbol.SetMethod.IsInitOnly)
            {
                //Create setter method
                body = body.AddStatements(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                                GenericName(Identifier("Action"))
                                    .WithTypeArgumentList(
                                        MakeTypeArgumentList(
                                            IdentifierName("TOut")
                                            )
                                        ))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier("setViewModel"))
                                        .WithInitializer(
                                            EqualsValueClause(
                                                ParenthesizedLambdaExpression()
                                                    .WithParameterList(
                                                        ParameterList(
                                                            SingletonSeparatedList(
                                                                Parameter(Identifier("value")))))
                                                    .WithExpressionBody(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("viewModel"),
                                                                IdentifierName(propertySymbol.Name)),
                                                            InvocationExpression(
                                                                    MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName("converter"),
                                                                        IdentifierName("ConvertFrom")))
                                                                .WithArgumentList(
                                                                    MakeArgumentList(
                                                                        Argument(IdentifierName("value")))
                                                                    )
                                                            )))))))
                );
                propertyBindingConstructorArguments.Add(Argument(IdentifierName("setViewModel")));
            }

            //Create an instance of PropertyBinding with the getter/setter if available.
            body = body.AddStatements(
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier("propertyBinding"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(
                                                    GenericName(Identifier("PropertyBinding"))
                                                        .WithTypeArgumentList(
                                                            MakeTypeArgumentList(
                                                                IdentifierName("TOut"),
                                                                IdentifierName("TWidget")
                                                                )
                                                            ))
                                                .WithArgumentList(
                                                    MakeArgumentList(
                                                        propertyBindingConstructorArguments.ToArray()
                                                        )
                                                        )
                                                    )))))
            );
            
            //Define Local Method - OnDisposed
            var onDisposedMethodBody = Block();
            
            if (canNotify)
            {
                (body, onDisposedMethodBody) = AddDisposedEvent(
                    body, onDisposedMethodBody, 
                    IdentifierName("viewModel"), 
                    IdentifierName("PropertyChanged"), 
                    IdentifierName("ViewModelOnPropertyChanged"));
            }


            var hasThingsToDispose = onDisposedMethodBody.Statements.Count > 0;
            if (hasThingsToDispose)
            {
                (body, onDisposedMethodBody) = AddDisposedEvent(
                    body, onDisposedMethodBody,
                    IdentifierName("widget"),
                    IdentifierName("Disposing"),
                    IdentifierName("OnDisposing"));

                body = body.AddStatements(LocalFunctionStatement(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier("OnDisposing"))
                    .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier("sender"))
                                .WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))),
                            Parameter(Identifier("e"))
                                .WithType(IdentifierName("EventArgs"))
                            ))
                    .WithBody(onDisposedMethodBody));
            }

            if (canNotify)
            {
                body = body.AddStatements(LocalFunctionStatement(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier("ViewModelOnPropertyChanged"))
                    .WithParameterList(
                        MakeParameterList(
                            Parameter(Identifier("sender"))
                                .WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))),
                            Parameter(Identifier("e"))
                                .WithType(IdentifierName("PropertyChangedEventArgs"))
                        ))
                    .WithBody(
                        Block(
                            IfStatement(
                                BinaryExpression(
                                    SyntaxKind.NotEqualsExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("e"),
                                        IdentifierName("PropertyName")),
                                    InvocationExpression(IdentifierName("nameof"))
                                        .WithArgumentList(
                                            MakeArgumentList(
                                                Argument(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName(classSymbol.Name),
                                                        IdentifierName(propertySymbol.Name))
                                                )))),
                                ReturnStatement()),
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("propertyBinding"),
                                        IdentifierName("NotifyViewModelPropertyChanged"))))))
                );
            }

            body = body.AddStatements(
                ReturnStatement(IdentifierName("propertyBinding"))
                );

            return method
                .WithBody(body);
        }

        private (BlockSyntax body, BlockSyntax onDisposedMethodBody) AddDisposedEvent(BlockSyntax body, BlockSyntax onDisposedMethodBody, IdentifierNameSyntax objectVariable, IdentifierNameSyntax eventName, IdentifierNameSyntax methodName)
        {
            var lhs = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objectVariable,
                eventName);
            return (
                body.AddStatements(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.AddAssignmentExpression,
                            lhs,
                            methodName))
                ),
                onDisposedMethodBody.AddStatements(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SubtractAssignmentExpression,
                            lhs,
                            methodName))
                )
            );
        }
    }
}