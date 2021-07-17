using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MLEM.Misc;
using MLEM.Ui;

namespace Slate.Client.UI.Views
{
    public static class RootElementExtensions
    {
        public static RootElement FadeOut(this RootElement element, TimeSpan? duration = null, Easings.Easing? easing = null, bool remove = false)
        {
            var time = duration ?? TimeSpan.FromSeconds(1);
            easing ??= Easings.InCubic;

            var startOpacity = element.Element.DrawAlpha;
            var sw = Stopwatch.StartNew();
            Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    var progress = (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    element.Element.DrawAlpha = opacity;
                    await RudeEngineGame.NextUpdate;
                }

                if (remove)
                {
                    element.System.Remove(element.Name);
                }
            });
            return element;
        }
    }
}