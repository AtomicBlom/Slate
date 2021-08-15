using Myra.Graphics2D.UI;

namespace Slate.Client.UI.Views
{
    internal static class WidgetExtensions
    {
        public static T AddChildren<T>(this T widget, params Widget[] children) where T : IMultipleItemsContainer
        {
            foreach (var child in children)
            {
                widget.AddChild(child);
            }

            return widget;
        }

        public static T RemoveAllChildren<T>(this T widget) where T : Myra.Graphics2D.UI.Container
        {
            if (widget is StackPanel sp)
            {
                sp.Widgets.Clear();
            }
            else
            {
                while (widget.ChildrenCount > 0)
                {
                    var child = widget.GetChild(0);
                    widget.RemoveChild(child);
                }
            }

            return widget;
        }
    }
}