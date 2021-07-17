namespace Slate.Client.UI.Views
{
    public class ToStringConverter : IConverter<object?, string?>
    {
        public string? Convert(object? value)
        {
            return value?.ToString();
        }
    }
}