using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Framework
{
    public record Rectangle(float Left, float Top, float Width, float Height)
    {
        public Rectangle(Vector2 location, Vector2 size) : this(location.X, location.Y, size.X, size.Y) { }

        public Vector2 Location => new(Left, Top);
        public Vector2 Size => new(Width, Height);

        public Rectangle Inflate(float x, float y)
        {
            return new Rectangle(Left - x, Top - y, Width - x - x, Height - y - y);
        }
    }

    public abstract class LayoutElement : UIElement
    {
        private bool needsArranging = true;
        private bool needsMeasuring = true;

        static void InvalidateArrange<T>(UIElement element, T oldValue, T newValue) =>
            ((LayoutElement)element).InvalidateArrange();

        static void InvalidateMeasure<T>(UIElement element, T oldValue, T newValue) =>
            ((LayoutElement)element).InvalidateMeasure();

        public static readonly UIProperty<VerticalAlignment> VerticalAlignmentProperty =
            new(nameof(VerticalAlignment), typeof(LayoutElement), VerticalAlignment.Top, InvalidateArrange);

        public VerticalAlignment VerticalAlignment
        {
            get => GetProperty(VerticalAlignmentProperty);
            set => SetProperty(VerticalAlignmentProperty, value);
        }

        public static readonly UIProperty<HorizontalAlignment> HorizontalAlignmentProperty =
            new(nameof(HorizontalAlignment), typeof(LayoutElement), HorizontalAlignment.Stretch, InvalidateArrange);

        public HorizontalAlignment HorizontalAlignment
        {
            get => GetProperty(HorizontalAlignmentProperty);
            set => SetProperty(HorizontalAlignmentProperty, value);
        }

        public static readonly UIProperty<float> WidthProperty =
            new(nameof(Width), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float Width
        {
            get => GetProperty(WidthProperty);
            set => SetProperty(WidthProperty, value);
        }

        public static readonly UIProperty<float> MinWidthProperty =
            new(nameof(MinWidth), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float MinWidth
        {
            get => GetProperty(MinWidthProperty);
            set => SetProperty(MinWidthProperty, value);
        }

        public static readonly UIProperty<float> MaxWidthProperty =
            new(nameof(MaxWidth), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float MaxWidth
        {
            get => GetProperty(MaxWidthProperty);
            set => SetProperty(MaxWidthProperty, value);
        }

        public static readonly UIProperty<float> HeightProperty =
            new(nameof(Height), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float Height
        {
            get => GetProperty(HeightProperty);
            set => SetProperty(HeightProperty, value);
        }

        public static readonly UIProperty<float> MinHeightProperty =
            new(nameof(MinHeight), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float MinHeight
        {
            get => GetProperty(MinHeightProperty);
            set => SetProperty(MinHeightProperty, value);
        }

        public static readonly UIProperty<float> MaxHeightProperty =
            new(nameof(MaxHeight), typeof(LayoutElement), float.NaN, InvalidateMeasure);

        public float MaxHeight
        {
            get => GetProperty(MaxHeightProperty);
            set => SetProperty(MaxHeightProperty, value);
        }

        public static readonly UIProperty<Thickness> PaddingProperty =
            new(nameof(Padding), typeof(LayoutElement), new Thickness(0, 0, 0, 0), InvalidateMeasure);

        public Thickness Padding
        {
            get => GetProperty(PaddingProperty);
            set => SetProperty(PaddingProperty, value);
        }

        public static readonly UIProperty<Thickness> MarginProperty =
            new(nameof(Margin), typeof(LayoutElement), new Thickness(0, 0, 0, 0), InvalidateMeasure);

        public Thickness Margin
        {
            get => GetProperty(MarginProperty);
            set => SetProperty(MarginProperty, value);
        }
        
        public void InvalidateArrange()
        {
            this.needsArranging = true;
            InvalidateArrangingChildren();
        }

        protected virtual void InvalidateArrangingChildren() { }

        public void InvalidateMeasure()
        {
            this.needsMeasuring = true;
            InvalidateMeasuringChildren();
        }

        protected virtual void InvalidateMeasuringChildren() { }

        public Vector2 DesiredSize { get; private set; } = Vector2.Zero;
    
        /// <summary>
        /// Calculates the minimum amount of space an element wishes to take up
        /// </summary>
        public void Measure()
        {
            if (!needsMeasuring) return;
            this.needsMeasuring = false;

            var overridenDesiredSize = MeasureOverride();
            if (!float.IsNaN(overridenDesiredSize.X) && !float.IsNaN(overridenDesiredSize.Y))
            {
            DesiredSize = overridenDesiredSize;
                return;
            }

            var desiredWidth = MinWidth;
            if (float.IsNaN(desiredWidth))
                desiredWidth = Width;
            if (float.IsNaN(desiredWidth))
                desiredWidth = 0;
            desiredWidth += Margin.Width + Padding.Width;


            var desiredHeight = MinHeight;
            if (float.IsNaN(desiredHeight))
                desiredHeight = Height;
            if (float.IsNaN(desiredHeight))
                desiredHeight = 0;
            desiredHeight += Margin.Height + Padding.Height;

            DesiredSize = new Vector2(desiredWidth, desiredHeight);
        }

        protected virtual Vector2 MeasureOverride() => new Vector2(float.NaN, float.NaN);

        public Vector2 RenderOffset { get; private set; } = Vector2.Zero;
        public Vector2 ActualSize { get; private set; } = Vector2.Zero;

        public void Arrange(Rectangle size)
        {
            if (!needsArranging) return;
            this.needsArranging = false;

            var overridenArrangement = ArrangeOverride(size);
            if (!float.IsNaN(overridenArrangement.Width) && !float.IsNaN(overridenArrangement.Height))
            {
                RenderOffset = overridenArrangement.Location;
                ActualSize = overridenArrangement.Size;
            }
            
            RenderOffset = new Vector2(
                size.Left + Margin.Left,
                size.Top + Margin.Top);
            ActualSize = new Vector2(
                HorizontalAlignment == HorizontalAlignment.Stretch ? size.Width - Margin.Width : DesiredSize.X,
                VerticalAlignment == VerticalAlignment.Stretch ? size.Height - Margin.Height : DesiredSize.Y
            );
        }

        protected readonly Rectangle DefaultArrangeBehaviour = new Rectangle(float.NaN, float.NaN, float.NaN, float.NaN);
        protected virtual Rectangle ArrangeOverride(Rectangle size) => DefaultArrangeBehaviour;

        protected bool SetValue<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            return true;
        }
    }
}