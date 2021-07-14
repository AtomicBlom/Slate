#nullable enable
using System;
using MLEM.Ui.Elements;

namespace BinaryVibrance.MLEM.Binding
{
    public class PropertyBinding<TNativePropertyType, TElement>
        where TElement : Element
    {
        public TElement Element { get; }
        public Func<TNativePropertyType> ViewModelGetter { get; }
        public Action<TNativePropertyType>? ViewModelSetter { get; }

        public event EventHandler<TNativePropertyType>? ViewModelPropertyChanged;

        public PropertyBinding(TElement element, Func<TNativePropertyType> viewModelGetter, Action<TNativePropertyType>? viewModelSetter = null)
        {
            Element = element;
            ViewModelGetter = viewModelGetter;
            ViewModelSetter = viewModelSetter;
        }

        public void NotifyViewModelPropertyChanged()
        {
            ViewModelPropertyChanged?.Invoke(this, ViewModelGetter());
        }
    }
}