using BinaryVibrance.MLEM.Binding;
using Slate.Client.ViewModel.MainMenu;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

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
                            .Bind(viewModel).Username().ToTextBox()
                        ,
                        new TextBox
                        {
                            GridColumn = 1, GridRow = 1,
                            PasswordField = true
                        }
                        .Bind(viewModel).Password().ToTextBox()
                        ,
                        new TextButton
                        {
                            GridColumn = 0, GridRow = 2, GridColumnSpan = 2, 
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = "Login"
                        }
                        .Bind(viewModel).LoginCommand().ToPressedEvent()
                        ,
                        new Label
                        {
                            GridColumn = 0, GridRow = 3, GridColumnSpan = 2,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            TextAlign = TextAlign.Center,
                            MaxWidth = 800
                        }
                            .Bind(viewModel).ErrorMessage().ToLabel()
                    )
            );
        }
    }
}