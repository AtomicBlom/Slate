using System;
using System.Collections.Generic;
using System.Windows.Input;
using Myra.Graphics2D.UI;
using Myra.Utility;
using Slate.Client.UI.Views;

namespace BinaryVibrance.MLEM.Binding
{
    public static partial class ElementBindingExtensions
    {
        public static TWidget ToTextBox<TWidget>(this PropertyBinding<string, TWidget> propertyBinding)
            where TWidget : TextBox
        {
            var element = propertyBinding.Widget;

            element.Disposing += OnDisposed;
            element.TextChanged += OnTextChange;
            propertyBinding.ViewModelPropertyChanged += ElementOnViewModelPropertyChanged;

            element.Text = propertyBinding.ViewModelGetter();

            void OnDisposed(object? sender, EventArgs args)
            {
                element.Disposing -= OnDisposed;
                element.TextChanged -= OnTextChange;
                propertyBinding.ViewModelPropertyChanged -= ElementOnViewModelPropertyChanged;
            }

            void OnTextChange(object? sender, ValueChangedEventArgs<string> args)
            {
                propertyBinding.ViewModelSetter?.Invoke(args.NewValue);
            }

            void ElementOnViewModelPropertyChanged(object? sender, string e)
            {
                element.Text = e;
            }

            return element;
        }

        public static TWidget ToLabel<TWidget>(this PropertyBinding<string, TWidget> propertyBinding)
            where TWidget : Label
        {
            var widget = propertyBinding.Widget;

            widget.Disposing += OnDisposed;
            propertyBinding.ViewModelPropertyChanged += ElementOnViewModelPropertyChanged;

            widget.Text = propertyBinding.ViewModelGetter();

            void OnDisposed(object? sender, EventArgs args)
            {
                widget.Disposing -= OnDisposed;
                propertyBinding.ViewModelPropertyChanged -= ElementOnViewModelPropertyChanged;
            }

            void ElementOnViewModelPropertyChanged(object? sender, string e)
            {
                widget.Text = e;
            }

            return widget;
        }

        public static TWidget ToPressedEvent<TWidget>(this PropertyBinding<ICommand, TWidget> propertyBinding, Func<object>? resolveParameter = null)
            where TWidget : Widget
        {
            var widget = propertyBinding.Widget;
            var command = propertyBinding.ViewModelGetter();

            widget.Disposing += OnDisposed;
            widget.TouchUp += OnPressed;
            command.CanExecuteChanged += CommandOnCanExecuteChanged;
            widget.Enabled = command.CanExecute(resolveParameter?.Invoke());

            void OnDisposed(object? sender, EventArgs args)
            {
                widget.Disposing -= OnDisposed;
                widget.TouchUp -= OnPressed;
                command.CanExecuteChanged -= CommandOnCanExecuteChanged;
            }

            void OnPressed(object? sender, EventArgs args)
            {
                var parameter = resolveParameter?.Invoke();
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }

            void CommandOnCanExecuteChanged(object? sender, EventArgs e)
            {
                widget.Enabled = command.CanExecute(resolveParameter?.Invoke());
            }

            return widget;
        }

        public static TElement ToRadioButton<TElement, TValue>(this PropertyBinding<TValue, TElement> propertyBinding, TValue matchingValue)
            where TElement : RadioButton
        {
            var widget = propertyBinding.Widget;

            widget.Disposing += OnDisposed;
            widget.PressedChanged += OnSelectedChanged;
            propertyBinding.ViewModelPropertyChanged += ElementOnViewModelPropertyChanged;

            void OnDisposed(object? sender, EventArgs args)
            {
                widget.Disposing -= OnDisposed;
                widget.PressedChanged -= OnSelectedChanged;
                propertyBinding.ViewModelPropertyChanged -= ElementOnViewModelPropertyChanged;

            }

            void OnSelectedChanged(object? sender, EventArgs eventArgs)
            {
                if (widget.IsPressed)
                {
                    propertyBinding?.ViewModelSetter(matchingValue);
                }
            }

            void ElementOnViewModelPropertyChanged(object? sender, TValue e)
            {
                if (Equals(e, matchingValue))
                {
                    widget.IsPressed = true;
                }
            }

            return widget;
        }

        public static TElement ToChildTemplate<TElement, TChildType>(this PropertyBinding<IEnumerable<TChildType>, TElement> propertyBinding, Func<TChildType, Widget> createChildAction)
            where TElement : Container, IMultipleItemsContainer
        {
            var element = propertyBinding.Widget;

            element.Disposing += OnDisposed;
            propertyBinding.ViewModelPropertyChanged += ElementOnViewModelPropertyChanged;
            
            ElementOnViewModelPropertyChanged(null, propertyBinding.ViewModelGetter());

            void OnDisposed(object? sender, EventArgs args)
            {
                element.Disposing -= OnDisposed;
                propertyBinding.ViewModelPropertyChanged -= ElementOnViewModelPropertyChanged;
            }

            void ElementOnViewModelPropertyChanged(object? sender, IEnumerable<TChildType> e)
            {
                element.RemoveAllChildren();
                foreach (var child in e)
                {
                    element.AddChild(createChildAction(child));
                }
            }

            return element;
        }
    }
}