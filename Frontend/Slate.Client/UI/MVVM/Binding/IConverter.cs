namespace Slate.Client.UI.Views
{
    public interface IConverter<in TSource, out TDestination>
    {
        public TDestination Convert(TSource value);
    }
}