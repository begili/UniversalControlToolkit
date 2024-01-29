using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctApplicationButton : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    private readonly Border _brdHost;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctApplicationButton()
    {
        var cp = new ContentPresenter();
        cp.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(Content)) { Source = this });
        cp.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding(nameof(ContentTemplate)) { Source = this });

        Viewbox vb = new Viewbox()
        {
            Child = cp,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(2)
        };

        _brdHost = new Border()
        {
            Child = vb
        };
        ToolTipService.SetInitialShowDelay(_brdHost, 0);
        _brdHost.SetBinding(Border.BackgroundProperty,
            new Binding(nameof(IsHighlighted)) { Source = this, Converter = new IsHighlightedToBackgroundConverter() });
        _brdHost.SetBinding(Border.ToolTipProperty, new Binding(nameof(ToolTip)) { Source = this });

        AddVisualChild(_brdHost);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    protected override int VisualChildrenCount => 1;

    public object Content
    {
        get => (object)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctApplicationButton),
            new PropertyMetadata(null));

    public DataTemplate ContentTemplate
    {
        get => (DataTemplate)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(nameof(ContentTemplate), typeof(DataTemplate), typeof(UctApplicationButton),
            new PropertyMetadata(null));

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(UctApplicationButton),
            new PropertyMetadata(false));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _brdHost;

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class IsHighlightedToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bVal && bVal)
                return Brushes.SkyBlue;
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}