using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MLEM.Misc;
using MLEM.Ui;

namespace Slate.Client.UI.Views
{
    public static class RootElementExtensions
    {
        public static RootElement FadeOut(this RootElement rootElement, TimeSpan? duration = null, Easings.Easing? easing = null, bool remove = false, bool disable = true)
        {
            if (disable)
            {
                rootElement.Element.CanBeMoused = false;
                rootElement.Element.CanBePressed = false;
                rootElement.Element.CanBeSelected = false;
            }

            var time = duration ?? TimeSpan.FromMilliseconds(500);
            easing ??= Easings.InCubic;

            var startOpacity = rootElement.Element.DrawAlpha;
            var sw = Stopwatch.StartNew();
            Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    var progress = 1 - (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    rootElement.Element.DrawAlpha = opacity;
                    await RudeEngineGame.NextUpdate;
                }

                if (remove)
                {
                    rootElement.System.Remove(rootElement.Name);
                }
                else
                {

                }
            });
            return rootElement;
        }
    }
}