using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctImageButton : Control
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

    public UctImageButton()
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
            new MultiBinding()
            {
                Converter = new IsSelectedHighlightedToBackgroundConverter(),
                Bindings =
                {
                    new Binding(nameof(IsSelected)) { Source = this },
                    new Binding(nameof(IsMouseOver)) { Source = this },
                    new Binding(nameof(SelectedBackground)) { Source = this },
                    new Binding(nameof(HighlightBackground)) { Source = this }
                }
            });
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
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctImageButton),
            new PropertyMetadata(null));

    public DataTemplate ContentTemplate
    {
        get => (DataTemplate)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(nameof(ContentTemplate), typeof(DataTemplate), typeof(UctImageButton),
            new PropertyMetadata(null));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(UctImageButton),
            new PropertyMetadata(false));

    public Brush HighlightBackground
    {
        get => (Brush)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }

    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.Register(nameof(HighlightBackground), typeof(Brush), typeof(UctImageButton),
            new PropertyMetadata(Brushes.LightBlue));

    public Brush SelectedBackground
    {
        get => (Brush)GetValue(SelectedBackgroundProperty);
        set => SetValue(SelectedBackgroundProperty, value);
    }

    public static readonly DependencyProperty SelectedBackgroundProperty =
        DependencyProperty.Register(nameof(SelectedBackground), typeof(Brush), typeof(UctImageButton),
            new PropertyMetadata(Brushes.SkyBlue));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _brdHost;

    //--------------------------
    //
    //      events
    //
    //--------------------------

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class IsSelectedHighlightedToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 4 || !(values[0] is bool isSelected) || !(values[1] is bool isMouseOver) ||
                !(values[2] is Brush brushSelected) || !(values[3] is Brush brushHighlighted))
                return Brushes.Transparent;
            if (isSelected)
                return brushSelected;
            if (isMouseOver)
                return brushHighlighted;
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}