namespace Slate.Client.UI.Framework
{
    public record Thickness(float Left = 0, float Top = 0, float Right = 0, float Bottom = 0)
    {
        public Thickness(float x, float y) : this(x, y, x, y) { }

        public Thickness(float thickness) : this(thickness, thickness, thickness, thickness) { }

        public float Width => Left + Right;
        public float Height => Top + Bottom;
    }
}