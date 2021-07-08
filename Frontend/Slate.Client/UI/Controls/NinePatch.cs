using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Media.Imaging;

namespace Slate.Client.UI.Controls
{
    public partial class NinePatch
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource), typeof(BitmapImage), typeof(NinePatch), new FrameworkPropertyMetadata(default(BitmapImage)));

        public BitmapImage ImageSource
        {
            get => (BitmapImage)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty NinePatchSizesProperty = DependencyProperty.Register(
            nameof(NinePatchSizes), typeof(Thickness), typeof(NinePatch), new FrameworkPropertyMetadata(default(Thickness)));

        public Thickness NinePatchSizes
        {
            get => (Thickness)GetValue(NinePatchSizesProperty);
            set => SetValue(NinePatchSizesProperty, value);
        }
    }
}
