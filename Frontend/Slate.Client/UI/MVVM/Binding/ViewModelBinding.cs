using MLEM.Ui.Elements;

namespace Slate.Client.UI.Views
{
    public record ViewModelBinding<TElement, TViewModel>(TElement Element, TViewModel ViewModel)
        where TElement : Element;
}