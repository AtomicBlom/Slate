using System;

namespace Slate.Client.UI.Framework
{
    public class UIProperty<T>
    {
        private readonly UIPropertyChanged<T>? _onChangedAction;

        public UIProperty(string name, Type owner, T defaultValue)
        {
            Key = new UIPropertyKey(typeof(T), name, owner);
            DefaultValue = () => defaultValue;
        }

        public UIProperty(string name, Type owner, Func<T> defaultValue)
        {
            Key = new UIPropertyKey(typeof(T), name, owner);
            DefaultValue = defaultValue;
        }

        public UIProperty(string name, Type owner, T defaultValue, UIPropertyChanged<T> onChangedAction)
        {
            _onChangedAction = onChangedAction;
            Key = new UIPropertyKey(typeof(T), name, owner);
            DefaultValue = () => defaultValue;
        }

        public UIProperty(string name, Type owner, Func<T> defaultValue, UIPropertyChanged<T> onChangedAction)
        {
            _onChangedAction = onChangedAction;
            Key = new UIPropertyKey(typeof(T), name, owner);
            DefaultValue = defaultValue;
        }

        public UIPropertyKey Key { get; }
        private Func<T> DefaultValue { get; }

        public T GetDefaultValue()
        {
            return DefaultValue();
        }

        public void Notify(UIElement uiElement, T previousValue, T newValue)
        {
            _onChangedAction?.Invoke(uiElement, previousValue, newValue);
        }
    }
}