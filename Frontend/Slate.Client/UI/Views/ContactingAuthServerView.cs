using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Slate.Client.ViewModel.MainMenu;
using BinaryVibrance.MLEM.Binding;
using MLEM.Formatting;
using Slate.Client.UI.MVVM.Binding;

namespace Slate.Client.UI.Views
{
    internal class ContactingAuthServerView
    {
        public static Element CreateView(ContactingAuthServerViewModel viewModel)
        {
            return new ReloadablePanel(Anchor.Center, new Vector2(800, 300), Vector2.Zero, true)
            {
                Build = p => RebuildView(p, viewModel),
                DrawAlpha = 0.5f
            };
        }

        private static void RebuildView(Panel panel, ContactingAuthServerViewModel viewModel)
        {
            panel.ChildPadding = new Padding(32, 32);
            panel.AddChildren(
                new Group(Anchor.AutoCenter, new Vector2(800, 32), false)
                    {
                        ChildPadding = new Padding(0, 0, 0, 8)
                    }
                    .AddChildren(
                        new Paragraph(Anchor.TopCenter, 1f, "Contacting Auth Server...")
                        {
                            Alignment = TextAlignment.Center
                        }
                    ),
                new Group(Anchor.AutoCenter, new Vector2(800, 64), false)
                    {
                        ChildPadding = new Padding(0, 0, 0, 8)
                    }
                    .AddChildren(
                        new  Paragraph(Anchor.TopCenter, 1f, string.Empty)
                            {
                                Alignment = TextAlignment.Center
                            }
                            .Bind(viewModel).ErrorMessage().ToParagraph()
                    )
            );
        }
    }
}