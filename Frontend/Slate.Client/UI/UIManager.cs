using System;
using Myra.Graphics2D.UI;
using StrongInject;

namespace Slate.Client.UI
{
    public class UIManager : IUIManager
    {
        private readonly Desktop _desktop;

        public UIManager(Desktop desktop)
        {
            _desktop = desktop;
        }

        public void ShowScreen<TView, TViewModel, TContainer>(TContainer container, TView viewFactory, Action<TViewModel>? configureViewModel = null)
            where TView : ViewFactory<TViewModel>
            where TContainer : IContainer<TViewModel>
        {
            var viewModel = container.Resolve();
            configureViewModel?.Invoke(viewModel.Value);
            var view = viewFactory.CreateView(viewModel.Value);
            view.Disposing += ViewOnDisposing;
            view.UserData.Add("ScreenName", typeof(TView).FullName);
            if (_desktop.Root is IMultipleItemsContainer screenContainer)
            {
                screenContainer.AddChild(view);
            }
            else
            {
                _desktop.Root?.Dispose();
                _desktop.Root = view;
            }
        
            void ViewOnDisposing(object? sender, EventArgs e)
            {
                view.Disposing -= ViewOnDisposing;
                viewModel.Dispose();
            }
        }
    }

    public interface IUIManager
    {
        public void ShowScreen<TView, TViewModel>(IContainer<TViewModel> container, TView viewFactory)
            where TView : ViewFactory<TViewModel>;
    }

    public abstract class ViewFactory<T>
    {
        public abstract Widget CreateView(T viewModel);
    }
}