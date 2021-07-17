#nullable enable
using System;
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
    