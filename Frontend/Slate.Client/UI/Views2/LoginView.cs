using System;
using Microsoft.Xna.Framework;
using Slate.Client.ViewModel.MainMenu;
using BinaryVibrance.MLEM.Binding;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Slate.Client.UI.MVVM.Binding;

namespace Slate.Client.UI.Views
{
    internal class ReloadablePanel2 : Panel
    {
        private readonly Action<Panel> _build;

        public ReloadablePanel2(Action<Panel> build)
        {
            _build = build;
            build(this);
        }

        public void Rebuild()
        {
            while (ChildrenCount > 0)
            {
                RemoveChild(GetChild(0));
            }

            _build(this);
        }
    }

    internal static class WidgetExtensions
    {
        public static T AddChildren<T>(this T widget, params Widget[] children) where T : IMultipleItemsContainer
        {
            foreach (var child in children)
            {
                widget.AddChild(child);
            }

            return widget;
        }
    }


    internal class LoginView2
    {
        public static Widget CreateView(LoginViewModel viewModel)
        {
            return new ReloadablePanel2(p => RebuildView(p, viewModel));
        }

        private static void RebuildView(Panel panel, LoginViewModel viewModel)
        {
            panel.Padding = new Thickness(32);
            panel.AddChildren(
                new Grid
                    {
                        ShowGridLines = true,
                        ColumnsProportions = { new(), new() },
                        RowsProportions = { new(), new(), new() },
                    }
                    .AddChildren(
                        new Label { Text = "Username:", GridColumn = 0, GridRow = 0 },
                        new Label { Text = "Password:", GridColumn = 0, GridRow = 1 },
                        new TextBox { GridColumn = 1, GridRow = 0 }
                            //.Bind(viewModel).Username().ToText()
                        ,
                        new TextBox
                        {
                            GridColumn = 1, GridRow = 1,
                            PasswordField = true
                        }
                        //.Bind(viewModel).Password().ToText()
                        ,
                        new TextButton
                        {
                            GridColumn = 0, GridRow = 2, GridColumnSpan = 2, 
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = "Login"
                        }
                        //.Bind(viewModel).LoginCommand().ToPressedEvent()
                    )
            );
        }
    }
}