using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Framework
{
    /// <summary>
    /// Base class for multiple children, by default it will just layer them all on top of each other.
    /// </summary>
    public class MultiChildElement : LayoutElement
    {

        public static readonly UIProperty<ObservableCollection<LayoutElement>> ChildrenProperty =
            new(nameof(Children), typeof(MultiChildElement), () => new(), OnChildCollectionChanged);

        private static void OnChildCollectionChanged(UIElement element, ObservableCollection<LayoutElement>? previousCollection, ObservableCollection<LayoutElement> newCollection)
        {
            var uiElement = (MultiChildElement)element;
            if (previousCollection is not null)
            {
                previousCollection.CollectionChanged -= uiElement.OnChildrenChanged;
            }

            newCollection.CollectionChanged += uiElement.OnChildrenChanged;
        }

        private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMeasure();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems is not null);
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is LayoutElement layoutElement)
                        {
                            layoutElement.SetParent(this);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems is not null);
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is LayoutElement layoutElement && layoutElement.Parent == this)
                        {
                            layoutElement.SetParent(null);
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException($"{e.Action} is not yet supported");
            }
        }

        public ObservableCollection<LayoutElement> Children => GetProperty(ChildrenProperty);

        protected override Vector2 MeasureOverride()
        {
            var minimumSize = Vector2.Zero;
            foreach (var child in Children)
            {
                child.Measure();
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
            var childSize = size.Deflate(Padding).Deflate(Margin);

            foreach (var child in Children)
            {
                child.Arrange(childSize);
            }

            return DefaultArrangeBehaviour;
        }
    }
}