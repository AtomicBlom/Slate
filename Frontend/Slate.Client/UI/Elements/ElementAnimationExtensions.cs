using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MLEM.Misc;
using MLEM.Ui.Elements;

namespace Slate.Client.UI.Elements
{
    public static class ElementAnimationExtensions
    {
        public static async Task FadeOutAsync(this Element element, TimeSpan? duration = null, Easings.Easing? easing = null, bool remove = false, bool disable = true, CancellationToken? cancellationToken = null)
        {
            if (disable)
            {
                element.CanBeMoused = false;
                element.CanBePressed = false;
                element.CanBeSelected = false;
            }

            var time = duration ?? TimeSpan.FromMilliseconds(500);
            easing ??= Easings.InCubic;

            var startOpacity = element.DrawAlpha;
            var sw = Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    if (cancellationToken?.IsCancellationRequested ?? false) return;
                    var progress = 1 - (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    element.DrawAlpha = opacity;
                    await RudeEngineGame.NextUpdate;
                }

                if (remove)
                {
                    if (element.Parent == null)
                    {
                        element.Root.System.Remove(element.Root.Name);
                    }
                    else
                    {
                        element.Parent.RemoveChild(element);
                    }
                }
            });
        }

        public static async Task FadeInAsync(this Element element, TimeSpan? duration = null, Easings.Easing? easing = null, bool disable = true, CancellationToken? cancellationToken = null)
        {
            if (disable)
            {
                element.CanBeMoused = false;
                element.CanBePressed = false;
                element.CanBeSelected = false;
            }

            var time = duration ?? TimeSpan.FromMilliseconds(500);
            easing ??= Easings.InCubic;

            var startOpacity = element.DrawAlpha;
            var sw = Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    if (cancellationToken?.IsCancellationRequested ?? false) return;
                    var progress = (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    element.DrawAlpha = opacity;
                    await RudeEngineGame.NextUpdate;
                }
            });
        }
    }
}