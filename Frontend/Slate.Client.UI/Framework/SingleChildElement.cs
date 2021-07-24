using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Framework
{
    public class SingleChildElement : LayoutElement
    {
        private LayoutElement content;

        public LayoutElement Content
        {
            get => this.content;
            set
            {
                if (SetValue(ref this.content, value))
                {
                    this.InvalidateMeasure();
                    if (this.content != null)
                    {
                        this.content.InvalidateMeasure();
                    }
                }
            }
        }

        protected override Vector2 MeasureOverride()
        {
            content?.Measure();
            var result = (content?.DesiredSize ?? Vector2.Zero) +
                         new Vector2(Margin.Width + Padding.Width, Margin.Height + Padding.Width);
            return result;
        }

        protected override Rectangle ArrangeOverride(Rectangle size)
        {


            var childSize = new Rectangle(size.Location, size.Size);
            childSize.Inflate(-(Margin.Width + Padding.Width), -(Margin.Height - Padding.Height));
            content?.Arrange(childSize);

            return DefaultArrangeBehaviour;
        }
    }
}