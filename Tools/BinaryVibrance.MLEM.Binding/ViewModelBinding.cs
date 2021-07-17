#nullable enable
using MLEM.Ui.Elements;

namespace BinaryVibrance.MLEM.Binding
{
    public record ViewModelBinding<TElement, TViewModel>(TElement Element, TViewModel ViewModel)
        where TElement : Element;

    public static partial class ElementBindingExtensions
    {
        public static ViewModelBinding<TElement, TViewModel> Bind<TElement, TViewModel>(this TElement element,
            TViewModel viewModel)
            where TElement : Element
        {
            return new ViewModelBinding<TElement, TViewModel>(element, viewModel);
        }
    }
}
    