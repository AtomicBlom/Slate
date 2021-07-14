#nullable enable
using MLEM.Ui.Elements;

namespace BinaryVibrance.MLEM.Binding
{
    public record ViewModelBinding<TElement, TViewModel>(TElement Element, TViewModel ViewModel)
        where TElement : Element;
}