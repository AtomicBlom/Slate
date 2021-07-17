using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Slate.Client.ViewModel.MainMenu;
using BinaryVibrance.MLEM.Binding;
using Slate.Client.UI.MVVM.Binding;

namespace Slate.Client.UI.Views
{
    internal class LoginView
    {
        public static Element CreateView(LoginViewModel viewModel)
        {
            return new ReloadablePanel(Anchor.Center, new Vector2(400, 100), Vector2.Zero, true)
            {
                Build = p => RebuildView(p, viewModel)
            };
        }

        private static void RebuildView(Panel panel, LoginViewModel viewModel)
        {
            panel.ChildPadding = new Padding(32, 32);
            panel.AddChildren(
                new Group(Anchor.AutoLeft, new Vector2(400))
                    {
                    ChildPadding = new Padding(0, 64, 0, 8)
                }
                    .AddChildren(
                        new Paragraph(Anchor.TopLeft, 0.3f, "Username: "),
                        new TextField(Anchor.AutoInlineIgnoreOverflow, new Vector2(0.7f, 40))
                            {
                                TextOffsetX = 12,
                            }
                            .Bind(viewModel).Username().ToText()
                    ),
                new Group(Anchor.AutoLeft, new Vector2(400))
                    {
                    ChildPadding = new Padding(0, 64, 0, 8)
                }
                    .AddChildren(
                        new Paragraph(Anchor.TopLeft, 0.3f, "Password: "),
                        new TextField(Anchor.AutoInlineIgnoreOverflow, new Vector2(0.7f, 40))
                            {
                                TextOffsetX = 12,
                                MaskingCharacter = '*'
                            }
                            .Bind(viewModel).Password().ToText()
                    )
                ,
                new Button(Anchor.AutoCenter, new Vector2(200, 48), "Log in")
                    {
                        Padding = new Padding(0, 0, 0, 0)
                    }
                    .Bind(viewModel).LoginCommand().ToPressedEvent()
            );
        }
    }
}