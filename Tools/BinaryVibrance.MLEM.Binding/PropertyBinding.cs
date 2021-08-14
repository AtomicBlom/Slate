#nullable enable
using System;
using Myra.Graphics2D.UI;

namespace BinaryVibrance.MLEM.Binding
{
    public class PropertyBinding<TNativePropertyType, TWidget>
        where TWidget : Widget
    {
        public TWidget Widget { get; }
        public Func<TNativePropertyType> ViewModelGetter { get; }
        public Action<TNativePropertyType>? ViewModelSetter { get; }

        public event EventHandler<TNativePropertyType>? ViewModelPropertyChanged;

        public PropertyBinding(TWidget widget, Func<TNativePropertyType> viewModelGetter, Action<TNativePropertyType>? viewModelSetter = null)
        {
            Widget = widget;
            ViewModelGetter = viewModelGetter;
            ViewModelSetter = viewModelSetter;
        }

        public void NotifyViewModelPropertyChanged()
        {
            ViewModelPropertyChanged?.Invoke(this, ViewModelGetter());
        }
    }
}