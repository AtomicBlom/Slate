using System;
using System.Linq;
using System.Threading.Tasks;
using CastIron.Engine;
using Myra.Graphics2D.UI;
using Slate.Client.UI.Elements;
using Slate.Client.ViewModel.MainMenu;
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

        public void ShowScreen<TViewModel>(IContainer<TViewModel> container, IViewFactory<TViewModel> viewFactory, Action<TViewModel>? configureViewModel = null)
        {
            var viewModel = container.Resolve();
            configureViewModel?.Invoke(viewModel.Value);
            if (viewModel.Value is INavigateTo navigable)
            {
                TaskDispatcher.FireAndForget(async () => await navigable.OnNavigatedTo());
            }
            var view = viewFactory.CreateView(viewModel.Value);
            view.Disposing += ViewOnDisposing;
            view.UserData.Add("ScreenName", viewFactory.GetType().FullName);
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

        public void RemoveScreen<TViewFactory>()
        {
            if (_desktop.Root is IMultipleItemsContainer screenContainer)
            {
                var screenToRemove = screenContainer.Widgets
                    .Where(w => w.UserData.ContainsKey("ScreenName"))
                    .FirstOrDefault(w => w.UserData["ScreenName"] == typeof(TViewFactory).FullName);
                screenContainer.Widgets.Remove(screenToRemove);
            }
            else
            {
                if (_desktop.Root.UserData.TryGetValue("ScreenName", out var existingScreenName) &&
                    existingScreenName == typeof(TViewFactory).FullName)
                {
                    _desktop.Root?.Dispose();
                    _desktop.Root = null;
                }
            }
        }

        public void FadeAndRemoveScreen<TViewFactory>()
        {
            Widget? widgetToFade = null;
            if (_desktop.Root is IMultipleItemsContainer screenContainer)
            {
                widgetToFade = screenContainer.Widgets
                    .Where(w => w.UserData.ContainsKey("ScreenName"))
                    .FirstOrDefault(w => w.UserData["ScreenName"] == typeof(TViewFactory).FullName);
            }
            else
            {
                if (_desktop.Root.UserData.TryGetValue("ScreenName", out var existingScreenName) &&
                    existingScreenName == typeof(TViewFactory).FullName)
                {
                    widgetToFade = _desktop.Root;
                }
            }

            if (widgetToFade is not null)
            {
                TaskDispatcher.FireAndForget(async () =>
                {
                    await widgetToFade.FadeOutAsync();
                    RemoveScreen<TViewFactory>();
                });
            }
        }
    }

    public interface IUIManager
    {
        public void ShowScreen<TViewModel>(
            IContainer<TViewModel> container, 
            IViewFactory<TViewModel> viewFactory,
            Action<TViewModel>? configureViewModel = null);

        void RemoveScreen<TViewFactory>();
        void FadeAndRemoveScreen<TViewFactory>();
    }

    public interface IViewFactory<in T>
    {
        public Widget CreateView(T viewModel);
    }
}