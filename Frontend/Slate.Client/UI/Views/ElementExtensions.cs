using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using MLEM.Ui.Elements;

namespace Slate.Client.UI.Views
{
    public static class ElementExtensions
    {
        public static T AddChildren<T>(this T parent, params Element[] children) where T : Element
        {
            foreach (var element in children) parent.AddChild(element);

            return parent;
        }

        public static T BindClick<T>(this T element, ICommand command, Func<object>? resolveParameter = null)
            where T : Element
        {
            command.CanExecuteChanged += CommandOnCanExecuteChanged;
            element.OnPressed += OnPressed;
            element.OnDisposed += OnDispose;

            CommandOnCanExecuteChanged(element, EventArgs.Empty);

            return element;

            void OnDispose(Element e)
            {
                command.CanExecuteChanged -= CommandOnCanExecuteChanged;
                e.OnPressed -= OnPressed;
                e.OnDisposed -= OnDispose;
            }

            void CommandOnCanExecuteChanged(object? sender, EventArgs e)
            {
                if (element is Button b) b.IsDisabled = !command.CanExecute(resolveParameter?.Invoke());
            }

            void OnPressed(Element e)
            {
                var parameter = resolveParameter?.Invoke();
                if (command.CanExecute(parameter)) command.Execute(parameter);
            }
        }

        //FIXME: Make a source generator so we can avoid the Expression crap.
        public static TextField BindText<TViewModel>(this TextField textField, TViewModel viewModel,
            Expression<Func<TViewModel, string>> property) where TViewModel : INotifyPropertyChanged
        {
            if (property.Body is not MemberExpression propertyExpression)
                throw new ArgumentException("Expected a property expression", nameof(property));
            if (propertyExpression.Member is not PropertyInfo propertyInfo)
                throw new ArgumentException("Expected a property expression", nameof(property));
            var setMethod = propertyInfo.SetMethod;
            var canSetOnViewModel = setMethod is not null && setMethod.IsPublic;

            var compiledProperty = property.Compile();

            if (canSetOnViewModel) textField.OnTextChange += OnTextChange;

            textField.OnDisposed += OnDispose;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            OnViewModelPropertyChanged(textField, new PropertyChangedEventArgs(propertyInfo.Name));

            return textField;

            void OnDispose(Element e)
            {
                textField.OnTextChange -= OnTextChange;
                e.OnDisposed -= OnDispose;
                viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyInfo.Name)
                {
                    var newText = compiledProperty.Invoke(viewModel);
                    if (textField.Text != newText) textField.SetText(newText);
                }
            }

            void OnTextChange(TextField field, string text)
            {
                setMethod?.Invoke(viewModel, new object?[] { text });
            }
        }
    }
}