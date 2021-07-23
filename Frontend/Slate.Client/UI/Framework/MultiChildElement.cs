using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Framework
{
    /// <summary>
    /// Base class for multiple children, by default it will just layer them all on top of each other.
    /// </summary>
    public class MultiChildElement : LayoutElement
    {

        public static readonly UIProperty<List<LayoutElement>> ChildrenProperty =
            new(nameof(Children), typeof(MultiChildElement), () => new());

        public IReadOnlyList<LayoutElement> Children => GetProperty(ChildrenProperty);

        protected override Vector2 MeasureOverride()
        {
            var minimumSize = Vector2.Zero;
            foreach (var child in Children)
            {
                child?.Measure();
                minimumSize = new Vector2(
                    MathF.Max(minimumSize.X, child.DesiredSize.X),
                    MathF.Max(minimumSize.Y, child.DesiredSize.Y)
                );
            }

            
            var result = minimumSize +
                         new Vector2(Margin.Width + Padding.Width, Margin.Height + Padding.Width);
            return result;
        }

        protected override Rectangle ArrangeOverride(Rectangle size)
        {
            var childSize = new Rectangle(size.Location, size.Size);
            childSize = childSize.Inflate(-(Margin.Width + Padding.Width), -(Margin.Height - Padding.Height));

            foreach (var child in Children)
            {
                child.Arrange(childSize);
            }

            

            return DefaultArrangeBehaviour;
        }

        public AddChild(LayoutElement child, int? index = null)
        {
            if (index > Children.Count) index = null;
            if (index is not null)
            {

            }
        }
    }
}