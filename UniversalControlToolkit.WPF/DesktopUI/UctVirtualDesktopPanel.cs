using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctVirtualDesktopPanel : Control
{
    //--------------------------
    //
    //      header
    //
    //--------------------------

    private readonly Border _brdBackground;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctVirtualDesktopPanel()
    {
        _brdBackground = new Border()
        {
            IsHitTestVisible = true
        };
        _brdBackground.SetBinding(Border.BackgroundProperty, new MultiBinding()
        {
            Converter = new BackgroundConverter(),
            Bindings =
            {
                new Binding(nameof(Background)) { Source = this },
                new Binding(nameof(BackgroundImage)) { Source = this }
            }
        });
        AddVisualChild(_brdBackground);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    protected override int VisualChildrenCount => 1;

    public string BackgroundImage
    {
        get => (string)GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }

    public static readonly DependencyProperty BackgroundImageProperty =
        DependencyProperty.Register(nameof(BackgroundImage), typeof(string), typeof(UctVirtualDesktopPanel),
            new PropertyMetadata(null));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _brdBackground;

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class BackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] != null && !(values[0] is Brush) ||
                values[1] != null && !(values[1] is string))
                return Brushes.Transparent;
            var bgBrush = values[0] as Brush ?? Brushes.Transparent;
            var filePath = values[1] as string;
            if (filePath != null && File.Exists(filePath))
                return new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(filePath, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            return bgBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}