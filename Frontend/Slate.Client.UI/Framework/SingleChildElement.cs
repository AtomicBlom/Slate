using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Framework
{
    public class SingleChildElement : LayoutElement
    {
        private LayoutElement? _content;

        public LayoutElement? Content
        {
            get => this._content;
            set
            {
                if (SetValue(ref this._content, value))
                {
                    this.InvalidateMeasure();
                    if (this._content != null)
                    {
                        this._content.InvalidateMeasure();
                    }
                }
            }
        }

        protected override Vector2 MeasureOverride()
        {
            _content?.Measure();
            var result = (_content?.DesiredSize ?? Vector2.Zero) +
                         new Vector2(Margin.Width + Padding.Width, Margin.Height + Padding.Width);
            return result;
        }

        protected override Rectangle ArrangeOverride(Rectangle size)
        {
            var childSize = new Rectangle(size.Location, size.Size);
            _content?.Arrange(childSize.Deflate(Margin).Deflate(Padding));
            return DefaultArrangeBehaviour;
        }
    }
}