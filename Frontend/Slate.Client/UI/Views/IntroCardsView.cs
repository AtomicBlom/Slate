using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.UI.Elements;

namespace Slate.Client.UI.Views
{
    class IntroCardsView
    {
        public static Element CreateView(IntroCardsViewModel viewModel)
        {
            var cards = new Element[]
            {
                //new Panel(Anchor.AutoLeft, new Vector2(400, 400), Vector2.Zero, setHeightBasedOnChildren:false),
                //new Paragraph(Anchor.AutoLeft, 400, "Steve wuz 'ere 2021 games", false),
                //new Paragraph(Anchor.AutoLeft, 400, "Concept and Graphics by Rosethethorn", false)
            };

            var cts = new CancellationTokenSource();

            var panel = new Group(Anchor.Center, new Vector2(400, 400), setHeightBasedOnChildren:false)
            {
                OnTextInput = (_, _, _) => cts.Cancel(),
                OnPressed = _ => cts.Cancel(),
                OnTouchEnter = _ => cts.Cancel()
            };

            cts.Token.Register(viewModel.Skip);

            Task.Run(async () =>
            {
                foreach (var card in cards)
                {
                    card.DrawAlpha = 0;
                    panel.AddChild(card);
                    await card.FadeInAsync(TimeSpan.FromSeconds(1), cancellationToken: cts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);

                    await card.FadeOutAsync(TimeSpan.FromSeconds(1), remove:true);
                    if (cts.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                viewModel.Finish();
            });

            return panel;
        }
    }
}
