namespace Slate.Client.UI.Framework
{
    public delegate void UIPropertyChanged<in T>(UIElement uiElement, T? oldValue, T newValue);
}