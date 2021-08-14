using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CastIron.Engine;
using MLEM.Misc;
using Myra.Graphics2D.UI;

namespace Slate.Client.UI.Elements
{
    public static class WidgetAnimationExtensions
    {
        public static async Task FadeOutAsync(this Widget element, TimeSpan? duration = null, Easings.Easing? easing = null, bool disable = true, CancellationToken? cancellationToken = null)
        {
            if (disable)
            {
                element.Enabled = false;
            }

            var time = duration ?? TimeSpan.FromMilliseconds(500);
            easing ??= Easings.InCubic;

            var startOpacity = element.Opacity;
            await TaskDispatcher.NextUpdate;
            var sw = Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    if (cancellationToken?.IsCancellationRequested ?? false) return;
                    var progress = 1 - (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    element.Opacity = opacity;
                    await TaskDispatcher.NextUpdate;
                }
            });
        }

        public static async Task FadeInAsync(this Widget element, TimeSpan? duration = null, Easings.Easing? easing = null, bool disable = true, CancellationToken? cancellationToken = null)
        {
            if (disable)
            {
                element.Enabled = false;
            }

            var time = duration ?? TimeSpan.FromMilliseconds(500);
            easing ??= Easings.InCubic;

            var startOpacity = element.Opacity;
            var sw = Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
                {
                    if (cancellationToken?.IsCancellationRequested ?? false) return;
                    var progress = (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

                    var easedProgress = easing(progress);
                    var opacity = startOpacity * easedProgress;

                    element.Opacity = opacity;
                    await TaskDispatcher.NextUpdate;
                }
            });
        }
    }
}