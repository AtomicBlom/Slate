using System;
using System.ComponentModel;
using System.Windows.Input;
using MLEM.Ui.Elements;
using Slate.Client.UI.ViewModels;

namespace Slate.Client.UI.Views
{
    public static class LoginViewBindingExtensions
    {
        public static PropertyBinding<string, TElement> Username<TElement>(this ViewModelBinding<TElement, LoginViewModel> viewModelBinding) where TElement : Element
        {
            var (element, viewModel) = viewModelBinding;

            Func<string> getViewModel = () => viewModel.Username;
            Action<string> setViewModel = (value) => viewModel.Username = value;
            var propertyBinding = new PropertyBinding<string, TElement>(element, getViewModel, setViewModel);

            element.OnDisposed += OnDisposed;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            
            void OnDisposed(Element _)
            {
                element.OnDisposed -= OnDisposed;
                viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            }

            void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(LoginViewModel.Username)) return;
                propertyBinding.NotifyViewModelPropertyChanged();
            }

            return propertyBinding;
        }

        public static PropertyBinding<string, TElement> Password<TElement>(this ViewModelBinding<TElement, LoginViewModel> viewModelBinding) where TElement : Element
        {
            var (element, viewModel) = viewModelBinding;

            Func<string> getViewModel = () => viewModel.Password;
            Action<string> setViewModel = (value) => viewModel.Password = value;
            var propertyBinding = new PropertyBinding<string, TElement>(element, getViewModel, setViewModel);

            element.OnDisposed += OnDisposed;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            void OnDisposed(Element _)
            {
                element.OnDisposed -= OnDisposed;
                viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            }

            void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(LoginViewModel.Password)) return;
                propertyBinding.NotifyViewModelPropertyChanged();
            }

            return propertyBinding;
        }

        public static PropertyBinding<ICommand, TElement> LoginCommand<TElement>(this ViewModelBinding<TElement, LoginViewModel> viewModelBinding) where TElement : Element
        {
            var (element, viewModel) = viewModelBinding;

            Func<ICommand> getViewModel = () => viewModel.LoginCommand;
            var propertyBinding = new PropertyBinding<ICommand, TElement>(element, getViewModel);

            element.OnDisposed += OnDisposed;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;

            void OnDisposed(Element _)
            {
                element.OnDisposed -= OnDisposed;
                viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            }

            void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != nameof(LoginViewModel.LoginCommand)) return;
                propertyBinding.NotifyViewModelPropertyChanged();
            }

            return propertyBinding;
        }
    }
}