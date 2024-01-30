using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctVirtualDesktopApplicationPanel : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    public event EventHandler<EventArgs> MaximizeRequested, MinimizeRequested, CloseRequested;

    private readonly Border _brdMainFrame, _brdContentFrame;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctVirtualDesktopApplicationPanel()
    {
        Grid headerGrid = new Grid()
        {
            IsHitTestVisible = true,
            Background = Brushes.Transparent,
            ColumnDefinitions =
            {
                new ColumnDefinition() { Width = GridLength.Auto },
                new ColumnDefinition() { Width = GridLength.Auto },
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition() { Width = GridLength.Auto },
                new ColumnDefinition() { Width = GridLength.Auto },
                new ColumnDefinition() { Width = GridLength.Auto },
            },
            Height = 32
        };

        UctImageButton btnMinimize = new UctImageButton();
        btnMinimize.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(MinimizedButtonTemplate)) { Source = this });
        btnMinimize.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(btnMinimize, 3);
        btnMinimize.MouseLeftButtonDown += (sender, args) => MinimizeRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(btnMinimize);

        UctImageButton btnMaximize = new UctImageButton();
        btnMaximize.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(MaximizedButtonTemplate)) { Source = this });
        btnMaximize.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(btnMaximize, 4);
        btnMaximize.MouseLeftButtonDown += (sender, args) => MaximizeRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(btnMaximize);

        UctImageButton btnClose = new UctImageButton();
        btnClose.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(CloseButtonTemplate)) { Source = this });
        btnClose.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(btnClose, 5);
        btnClose.MouseLeftButtonDown += (sender, args) => CloseRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(btnClose);

        Grid hostGrid = new Grid()
        {
            RowDefinitions =
            {
                new RowDefinition() { Height = GridLength.Auto },
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }
            }
        };
        hostGrid.Children.Add(headerGrid);

        ContentPresenter cp = new ContentPresenter();
        cp.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(Content)) { Source = this });

        Border brdContentBackground = new Border()
        {
            Child = cp,
            Margin = new Thickness(5)
        };
        brdContentBackground.SetBinding(Border.BackgroundProperty,
            new Binding(nameof(ContentBackground)) { Source = this });

        Grid.SetRow(brdContentBackground, 1);
        hostGrid.Children.Add(brdContentBackground);

        _brdMainFrame = new Border()
        {
            Child = hostGrid
        };
        _brdMainFrame.SetBinding(Border.BackgroundProperty, new Binding(nameof(Background)) { Source = this });
        AddVisualChild(_brdMainFrame);
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
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctVirtualDesktopApplicationPanel),
            new PropertyMetadata(null));


    public bool IsMaximized
    {
        get => (bool)GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    public static readonly DependencyProperty IsMaximizedProperty =
        DependencyProperty.Register(nameof(IsMaximized), typeof(bool), typeof(UctVirtualDesktopApplicationPanel),
            new PropertyMetadata(false));

    public DataTemplate CloseButtonTemplate
    {
        get => (DataTemplate)GetValue(CloseButtonTemplateProperty);
        set => SetValue(CloseButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty CloseButtonTemplateProperty =
        DependencyProperty.Register(nameof(CloseButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopApplicationPanel),
            new PropertyMetadata(null));

    public DataTemplate MaximizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MaximizedButtonTemplateProperty);
        set => SetValue(MaximizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MaximizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MaximizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopApplicationPanel), new PropertyMetadata(null));

    public DataTemplate MinimizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MinimizedButtonTemplateProperty);
        set => SetValue(MinimizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MinimizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MinimizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopApplicationPanel), new PropertyMetadata(null));

    public DataTemplate Icon
    {
        get => (DataTemplate)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(DataTemplate), typeof(UctVirtualDesktopApplicationPanel),
            new PropertyMetadata(null));

    public string ApplicationTitle
    {
        get => (string)GetValue(ApplicationTitlePropertyKey.DependencyProperty);
        protected set => SetValue(ApplicationTitlePropertyKey, value);
    }

    private static readonly DependencyPropertyKey ApplicationTitlePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ApplicationTitle), typeof(string),
            typeof(UctVirtualDesktopApplicationPanel), new PropertyMetadata(null));

    public Brush ContentBackground
    {
        get => (Brush)GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(UctVirtualDesktopApplicationPanel),
            new PropertyMetadata(Brushes.Transparent));

    public Brush HeaderButtonHighlightBackground
    {
        get => (Brush)GetValue(HeaderButtonHighlightBackgroundProperty);
        set => SetValue(HeaderButtonHighlightBackgroundProperty, value);
    }

    public static readonly DependencyProperty HeaderButtonHighlightBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderButtonHighlightBackground), typeof(Brush),
            typeof(UctVirtualDesktopApplicationPanel), new PropertyMetadata(Brushes.SkyBlue));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _brdMainFrame;
}