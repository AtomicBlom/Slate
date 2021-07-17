using System;
using System.Windows.Input;
using MLEM.Ui.Elements;

namespace BinaryVibrance.MLEM.Binding
{
    public static partial class ElementBindingExtensions
    {
        public static TElement ToText<TElement>(this PropertyBinding<string, TElement> propertyBinding)
            where TElement : TextField
        {
            var element = propertyBinding.Element;

            element.OnDisposed += OnDisposed;
            element.OnTextChange += OnTextChange;
            propertyBinding.ViewModelPropertyChanged += ElementOnViewModelPropertyChanged;

            element.SetText(propertyBinding.ViewModelGetter());

            void OnDisposed(Element _)
            {
                element.OnDisposed -= OnDisposed;
                element.OnTextChange -= OnTextChange;
                propertyBinding.ViewModelPropertyChanged -= ElementOnViewModelPropertyChanged;
            }

            void OnTextChange(TextField field, string text)
            {
                propertyBinding.ViewModelSetter?.Invoke(text);
            }

            void ElementOnViewModelPropertyChanged(object? sender, string e)
            {
                element.SetText(e);
            }

            return element;
        }

        public static TElement ToPressedEvent<TElement>(this PropertyBinding<ICommand, TElement> propertyBinding, Func<object>? resolveParameter = null)
            where TElement : Element
        {
            var element = propertyBinding.Element;
            var command = propertyBinding.ViewModelGetter();

            element.OnDisposed += OnDisposed;
            element.OnPressed += OnPressed;
            command.CanExecuteChanged += CommandOnCanExecuteChanged;
            element.CanBePressed = command.CanExecute(resolveParameter?.Invoke());
            
            void OnDisposed(Element _)
            {
                element.OnDisposed -= OnDisposed;
                element.OnPressed -= OnPressed;
                command.CanExecuteChanged -= CommandOnCanExecuteChanged;
            }

            void OnPressed(Element _)
            {
                var parameter = resolveParameter?.Invoke();
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }

            void CommandOnCanExecuteChanged(object? sender, EventArgs e)
            {
                element.CanBePressed = command.CanExecute(resolveParameter?.Invoke());
            }

            return element;
        }
    }
}