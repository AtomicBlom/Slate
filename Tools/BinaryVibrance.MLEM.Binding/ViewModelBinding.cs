#nullable enable
using System;
using Myra.Graphics2D.UI;

namespace BinaryVibrance.MLEM.Binding
{
    public record ViewModelBinding<TWidget, TViewModel>(TWidget Widget, TViewModel ViewModel)
        where TWidget : Widget;

    public static partial class ElementBindingExtensions
    {
        public static ViewModelBinding<TWidget, TViewModel> Bind<TWidget, TViewModel>(this TWidget widget,
            TViewModel viewModel)
            where TWidget : Widget
        {
            return new ViewModelBinding<TWidget, TViewModel>(widget, viewModel);
        }
    }

    public interface IConverter<TSource, TDestination>
    {
        TDestination ConvertTo(TSource value) => throw new NotSupportedException($"Converting from {typeof(TSource)} to {typeof(TDestination)} is not supported");
        TSource ConvertFrom(TDestination value) => throw new NotSupportedException($"Converting from {typeof(TDestination)} to {typeof(TSource)} is not supported");
    }

    public class IdentityConverter<T> : IConverter<T, T>
    {
        public T ConvertTo(T value) => value;
        public T ConvertFrom(T value) => value;
    }

    public class ToStringConverter<T> : IConverter<T, string>
    {
        public string ConvertTo(T value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}
    