using BinaryVibrance.MLEM.Binding;
using Slate.Client.ViewModel.MainMenu;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Styles;

namespace Slate.Client.UI.Views
{
    internal class LoginView : IViewFactory<LoginViewModel>
    {
        public Widget CreateView(LoginViewModel viewModel)
        {
            return new ReloadablePanel(p => RebuildView(p, viewModel));
        }

        private void RebuildView(Panel panel, LoginViewModel viewModel)
        {
            panel.Padding = new Thickness(32);
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Center;
            var children = new Window("static-panel")
            {
                Background = Stylesheet.Current.WindowStyle.Background,
                Content = new Grid
                    {
                        ColumnsProportions = { new(), new() },
                        RowsProportions = { new(), new(), new() },
                    }
                    .AddChildren(
                        new Label { Text = "Username:", GridColumn = 0, GridRow = 0, VerticalAlignment = VerticalAlignment.Center},
                        new Label { Text = "Password:", GridColumn = 0, GridRow = 1, VerticalAlignment = VerticalAlignment.Center},
                        new TextBox { GridColumn = 1, GridRow = 0 }
                            .Bind(viewModel).Username().ToTextBox()
                        ,
                        new TextBox
                            {
                                GridColumn = 1,
                                GridRow = 1,
                                PasswordField = true
                            }
                            .Bind(viewModel).Password().ToTextBox()
                        ,
                        new TextButton
                            {
                                GridColumn = 0,
                                GridRow = 2,
                                GridColumnSpan = 2,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Text = "Login"
                            }
                            .Bind(viewModel).LoginCommand().ToPressedEvent()
                        ,
                        new Label
                            {
                                GridColumn = 0,
                                GridRow = 3,
                                GridColumnSpan = 2,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                TextAlign = TextAlign.Center,
                                MaxWidth = 800
                            }
                            .Bind(viewModel).ErrorMessage().ToLabel()
                    )
            };
            children.CloseButton.Visible = false;
            panel.AddChildren(
                children
            );
        }
    }
}