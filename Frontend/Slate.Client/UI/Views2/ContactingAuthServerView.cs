using BinaryVibrance.MLEM.Binding;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Slate.Client.ViewModel.MainMenu;
using Myra.Graphics2D.UI;

namespace Slate.Client.UI.Views
{
    internal class ContactingAuthServerView : IViewFactory<ContactingAuthServerViewModel>
    {
        public Widget CreateView(ContactingAuthServerViewModel viewModel)
        {
            return new ReloadablePanel(p => RebuildView(p, viewModel));
        }

        private static void RebuildView(Panel panel, ContactingAuthServerViewModel viewModel)
        {
            panel.Padding = new Thickness(32);
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Center;
                
            panel.AddChildren(
                new VerticalStackPanel()
                    .AddChildren(
                        new Label { Text = "Contacting Auth Server...", HorizontalAlignment = HorizontalAlignment.Center },
                        new Label { Text = string.Empty, HorizontalAlignment = HorizontalAlignment.Center }
                            .Bind(viewModel).ErrorMessage().ToLabel()
                        )

            );
        }
    }
}