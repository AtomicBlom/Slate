using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client.UI.Controls
{
    /// <summary>
    /// Interaction logic for NinePatch.xaml
    /// </summary>
    public partial class NinePatch : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource), typeof(BitmapImage), typeof(NinePatch), new PropertyMetadata(default(BitmapImage)));

        public BitmapImage ImageSource
        {
            get => (BitmapImage)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty NinePatchSizesProperty = DependencyProperty.Register(
            nameof(NinePatchSizes), typeof(Thickness), typeof(NinePatch), new PropertyMetadata(default(Thickness)));

        public Thickness NinePatchSizes
        {
            get => (Thickness)GetValue(NinePatchSizesProperty);
            set => SetValue(NinePatchSizesProperty, value);
        }

        public NinePatch()
        {
            InitializeComponent();
        }
    }
}
