namespace Slate.Client.UI.Framework
{
    public record Thickness(float Left, float Top, float Right, float Bottom)
    {
        public float Width => Left + Right;
        public float Height => Top + Bottom;
    }
}