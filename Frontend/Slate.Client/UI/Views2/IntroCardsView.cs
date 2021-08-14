using System;
using System.Threading;
using System.Threading.Tasks;
using Myra.Graphics2D.UI;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.UI.Elements;

namespace Slate.Client.UI.Views
{
    class IntroCardsView2 : ViewFactory<IntroCardsViewModel>
    {
        public override Widget CreateView(IntroCardsViewModel viewModel)
        {
            var cards = new Widget[]
            {
                new Label { Text = "Steve wuz 'ere 2021 games", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center},
                new Label { Text = "Concept and Graphics by Rosethethorn", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center}
            };

            var cts = new CancellationTokenSource();

            var panel = new Panel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            panel.KeyUp += (_, _) => cts.Cancel();
            panel.TouchEntered += (_, _) => cts.Cancel();
            panel.TouchUp += (_, _) => cts.Cancel();

            cts.Token.Register(viewModel.Skip);

            Task.Run(async () =>
            {
                foreach (var card in cards)
                {
                    card.Opacity = 0;
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
