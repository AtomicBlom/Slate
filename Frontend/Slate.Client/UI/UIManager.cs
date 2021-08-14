using System;
using Myra.Graphics2D.UI;
using StrongInject;

namespace Slate.Client.UI
{
    public class UIManager
    {
        private readonly Desktop _desktop;

        public UIManager(Desktop desktop)
        {
            _desktop = desktop;
        }

        public void ShowScreen<TView, TViewModel, TContainer>(TContainer container, TView viewFactory)
            where TView : IViewFactory<TViewModel>
            where TContainer : IContainer<TViewModel>
        {
            var viewModel = container.Resolve();
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

    public interface IViewFactory<T>
    {
        Widget CreateView(T viewModel);
    }
}