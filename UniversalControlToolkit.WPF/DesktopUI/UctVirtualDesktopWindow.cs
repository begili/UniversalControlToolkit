using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using UniversalControlToolkit.WPF.DesktopUI.Utils;

namespace UniversalControlToolkit.WPF.DesktopUI;

[ContentProperty(nameof(Content))]
public class UctVirtualDesktopWindow : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    public event EventHandler<EventArgs> MaximizeRequested,
        MinimizeRequested,
        CloseRequested,
        Activated,
        GlobalMouseOperationFinished;

    public event EventHandler<GlobalMouseOperationEventArgs> GlobalMouseOperationStarted;

    private readonly Border _brdMainFrame, _brdContentFrame;
    private bool _isMouseDown;
    private MouseDownMode? _mouseDownMode;
    private Point? _parentMouseDownPoint;
    private Thickness? _mouseDownMargin;
    private double _mouseDownWidth, _mouseDownHeight;
    private readonly UctImageButton _btnMinimize, _btnMaximize, _btnClose;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    static UctVirtualDesktopWindow()
    {
        MinHeightProperty.OverrideMetadata(typeof(UctVirtualDesktopWindow),
            new FrameworkPropertyMetadata(100.0));
        MinWidthProperty.OverrideMetadata(typeof(UctVirtualDesktopWindow),
            new FrameworkPropertyMetadata(100.0));
    }

    public UctVirtualDesktopWindow()
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
        headerGrid.SetBinding(Grid.MarginProperty, new Binding(nameof(IsMaximized))
            { Source = this, Converter = new HeaderMarginConverter() });

        ContentPresenter cpIcon = new ContentPresenter();
        cpIcon.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding(nameof(Icon)) { Source = this });

        Viewbox vbIcon = new Viewbox()
        {
            Height = 32,
            Width = 32,
            Child = cpIcon,
            Margin = new Thickness(0, 0, 5, 0)
        };
        headerGrid.Children.Add(vbIcon);

        TextBlock tbTitle = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false };
        tbTitle.SetBinding(TextBlock.TextProperty, new Binding(nameof(ApplicationTitle)) { Source = this });
        Grid.SetColumn(tbTitle, 1);
        headerGrid.Children.Add(tbTitle);

        _btnMinimize = new UctImageButton();
        _btnMinimize.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(MinimizedButtonTemplate)) { Source = this });
        _btnMinimize.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(_btnMinimize, 3);
        _btnMinimize.MouseLeftButtonDown += (sender, args) => MinimizeRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(_btnMinimize);

        _btnMaximize = new UctImageButton();
        _btnMaximize.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(MaximizedButtonTemplate)) { Source = this });
        _btnMaximize.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(_btnMaximize, 4);
        _btnMaximize.MouseLeftButtonDown += (sender, args) => MaximizeRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(_btnMaximize);

        _btnClose = new UctImageButton();
        _btnClose.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(CloseButtonTemplate)) { Source = this });
        _btnClose.SetBinding(UctImageButton.HighlightBackgroundProperty,
            new Binding(nameof(HeaderButtonHighlightBackground)) { Source = this });
        Grid.SetColumn(_btnClose, 5);
        _btnClose.MouseLeftButtonDown += (sender, args) => CloseRequested?.Invoke(this, new EventArgs());
        headerGrid.Children.Add(_btnClose);

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
        SetSizes();
        AddVisualChild(_brdMainFrame);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    public bool IsSingleInstance
    {
        get => (bool)GetValue(IsSingleInstanceProperty);
        set => SetValue(IsSingleInstanceProperty, value);
    }

    public static readonly DependencyProperty IsSingleInstanceProperty =
        DependencyProperty.Register(nameof(IsSingleInstance), typeof(bool), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(false));

    public string? AppKey
    {
        get => (string?)GetValue(AppKeyProperty);
        set => SetValue(AppKeyProperty, value);
    }

    public static readonly DependencyProperty AppKeyProperty =
        DependencyProperty.Register(nameof(AppKey), typeof(string), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(null));

    protected override int VisualChildrenCount => 1;

    public object Content
    {
        get => (object)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(null));

    public DataTemplate CloseButtonTemplate
    {
        get => (DataTemplate)GetValue(CloseButtonTemplateProperty);
        set => SetValue(CloseButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty CloseButtonTemplateProperty =
        DependencyProperty.Register(nameof(CloseButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(null));

    public DataTemplate MaximizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MaximizedButtonTemplateProperty);
        set => SetValue(MaximizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MaximizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MaximizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopWindow), new PropertyMetadata(null));

    public DataTemplate MinimizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MinimizedButtonTemplateProperty);
        set => SetValue(MinimizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MinimizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MinimizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktopWindow), new PropertyMetadata(null));

    public DataTemplate Icon
    {
        get => (DataTemplate)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(DataTemplate), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(null));

    public string ApplicationTitle
    {
        get => (string)GetValue(ApplicationTitleProperty);
        set => SetValue(ApplicationTitleProperty, value);
    }

    public static readonly DependencyProperty ApplicationTitleProperty =
        DependencyProperty.Register(nameof(ApplicationTitle), typeof(string), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(null));

    public Brush ContentBackground
    {
        get => (Brush)GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(Brushes.Transparent));

    public Brush HeaderButtonHighlightBackground
    {
        get => (Brush)GetValue(HeaderButtonHighlightBackgroundProperty);
        set => SetValue(HeaderButtonHighlightBackgroundProperty, value);
    }

    public static readonly DependencyProperty HeaderButtonHighlightBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderButtonHighlightBackground), typeof(Brush),
            typeof(UctVirtualDesktopWindow), new PropertyMetadata(Brushes.SkyBlue));

    public bool IsMaximized
    {
        get => (bool)GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    public static readonly DependencyProperty IsMaximizedProperty =
        DependencyProperty.Register(nameof(IsMaximized), typeof(bool), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(false, OnIsMaximizedChanged));

    private static void OnIsMaximizedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktopWindow;
        snd.SetSizes();
    }

    public double DesiredWidth
    {
        get => (double)GetValue(DesiredWidthProperty);
        set => SetValue(DesiredWidthProperty, value);
    }

    public static readonly DependencyProperty DesiredWidthProperty =
        DependencyProperty.Register(nameof(DesiredWidth), typeof(double), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(double.NaN, OnDesiredWidthChanged));

    private static void OnDesiredWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktopWindow;
        snd.SetSizes();
    }

    public double DesiredHeight
    {
        get => (double)GetValue(DesiredHeightProperty);
        set => SetValue(DesiredHeightProperty, value);
    }

    public static readonly DependencyProperty DesiredHeightProperty =
        DependencyProperty.Register(nameof(DesiredHeight), typeof(double), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(double.NaN, OnDesiredHeightChanged));

    private static void OnDesiredHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktopWindow;
        snd.SetSizes();
    }

    public Thickness DesiredMargin
    {
        get => (Thickness)GetValue(DesiredMarginProperty);
        set => SetValue(DesiredMarginProperty, value);
    }

    public static readonly DependencyProperty DesiredMarginProperty =
        DependencyProperty.Register(nameof(DesiredMargin), typeof(Thickness), typeof(UctVirtualDesktopWindow),
            new PropertyMetadata(new Thickness(0), OnDesiredMarginChanged));

    private static void OnDesiredMarginChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktopWindow;
        snd.SetSizes();
    }

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    public void Show()
    {
        if (UctVirtualDesktop._currentDesktop == null)
            throw new InvalidOperationException("Cannot create a new Window without the Virtual Desktop");
        UctVirtualDesktop._currentDesktop.ShowWindow(this);
    }

    protected override Visual GetVisualChild(int index) => _brdMainFrame;

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        Activated?.Invoke(this, new EventArgs());
        base.OnPreviewMouseDown(e);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!_isMouseDown && _mouseDownMode != null && !_btnMinimize.IsMouseOver && !_btnMaximize.IsMouseOver &&
            !_btnClose.IsMouseOver)
        {
            var args = new GlobalMouseOperationEventArgs() { DesiredCursor = Cursor };
            GlobalMouseOperationStarted?.Invoke(this, args);

            if (!args.Handled || args.MouseHost == null)
                return;

            args.MouseHost.MouseMove += MouseHost_OnMouseMove;
            args.MouseHost.MouseLeave += MouseHost_OnMouseLeave;
            args.MouseHost.PreviewMouseLeftButtonUp += MouseHost_OnPreviewMouseLeftButtonUp;

            _isMouseDown = true;
            _parentMouseDownPoint = e.GetPosition(args.MouseHost);
            _mouseDownMargin = Margin;
            _mouseDownWidth = double.IsNaN(DesiredWidth) ? ActualWidth : DesiredWidth;
            _mouseDownHeight = double.IsNaN(DesiredHeight) ? ActualHeight : DesiredHeight;
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    private void DetachFromGlobalMouseHost(object sender)
    {
        _isMouseDown = false;

        var mouseHost = sender as IInputElement;
        if (mouseHost != null)
        {
            mouseHost.MouseMove -= MouseHost_OnMouseMove;
            mouseHost.MouseLeave -= MouseHost_OnMouseLeave;
            mouseHost.PreviewMouseLeftButtonUp -= MouseHost_OnPreviewMouseLeftButtonUp;
        }

        GlobalMouseOperationFinished?.Invoke(this, new EventArgs());
    }

    protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this);

        if (position.Y < (IsMaximized ? 32 : 37))
        {
            IsMaximized = !IsMaximized;
            e.Handled = true;
        }
        else
        {
            base.OnPreviewMouseDoubleClick(e);
        }
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (!_isMouseDown)
        {
            var pt = e.GetPosition(this);
            _mouseDownMode = null;
            if (!IsMaximized)
            {
                if (pt.X < 5 && pt.X > 1)
                {
                    _mouseDownMode = MouseDownMode.ResizeLeft;
                }
                else if (pt.X > ActualWidth - 5 && pt.X < ActualWidth - 1)
                {
                    _mouseDownMode = MouseDownMode.ResizeRight;
                }

                if (pt.Y < 5 && pt.Y > 1)
                {
                    switch (_mouseDownMode)
                    {
                        case MouseDownMode.ResizeLeft:
                            _mouseDownMode = MouseDownMode.ResizeTopLeft;
                            break;
                        case MouseDownMode.ResizeRight:
                            _mouseDownMode = MouseDownMode.ResizeTopRight;
                            break;
                        default:
                            _mouseDownMode = MouseDownMode.ResizeTop;
                            break;
                    }
                }
                else if (pt.Y > ActualHeight - 5 && pt.Y < ActualHeight - 1)
                {
                    switch (_mouseDownMode)
                    {
                        case MouseDownMode.ResizeLeft:
                            _mouseDownMode = MouseDownMode.ResizeBottomLeft;
                            break;
                        case MouseDownMode.ResizeRight:
                            _mouseDownMode = MouseDownMode.ResizeBottomRight;
                            break;
                        default:
                            _mouseDownMode = MouseDownMode.ResizeBottom;
                            break;
                    }
                }
            }

            if (_mouseDownMode == null && pt.Y < (IsMaximized ? 32 : 37))
            {
                _mouseDownMode = MouseDownMode.DragHeader;
            }

            switch (_mouseDownMode)
            {
                case MouseDownMode.ResizeBottom:
                case MouseDownMode.ResizeTop:
                    Cursor = Cursors.SizeNS;
                    break;
                case MouseDownMode.ResizeLeft:
                case MouseDownMode.ResizeRight:
                    Cursor = Cursors.SizeWE;
                    break;
                case MouseDownMode.ResizeTopRight:
                case MouseDownMode.ResizeBottomLeft:
                    Cursor = Cursors.SizeNESW;
                    break;
                case MouseDownMode.ResizeTopLeft:
                case MouseDownMode.ResizeBottomRight:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = null;
                    break;
            }
        }

        base.OnPreviewMouseMove(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        _isMouseDown = false;
        base.OnMouseLeave(e);
    }

    private void SetSizes()
    {
        if (IsMaximized)
        {
            Height = double.NaN;
            Width = double.NaN;
            Margin = new Thickness(0);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }
        else
        {
            Height = DesiredHeight;
            Width = DesiredWidth;
            Margin = DesiredMargin;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
        }
    }

    private void MouseHost_OnMouseMove(object sender, MouseEventArgs e)
    {
        var inputParent = sender as IInputElement;
        if (inputParent != null && _mouseDownMode != null)
        {
            var parentPoint = e.GetPosition(inputParent);
            var dist = parentPoint - _parentMouseDownPoint;
            if (dist.Value.Length < 3)
                return;
            IsMaximized = false;
            switch (_mouseDownMode)
            {
                case MouseDownMode.DragHeader:
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left + (int)dist.Value.X,
                        _mouseDownMargin.Value.Top + (int)dist.Value.Y, 0, 0);
                    break;
                case MouseDownMode.ResizeLeft:
                    DesiredWidth = Math.Max(_mouseDownWidth - (int)dist.Value.X, 0);
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left + (int)dist.Value.X,
                        _mouseDownMargin.Value.Top, 0, 0);
                    break;
                case MouseDownMode.ResizeTopLeft:
                    DesiredWidth = Math.Max(_mouseDownWidth - (int)dist.Value.X, 0);
                    DesiredHeight = Math.Max(_mouseDownHeight - (int)dist.Value.Y, 0);
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left + (int)dist.Value.X,
                        _mouseDownMargin.Value.Top + (int)dist.Value.Y, 0, 0);
                    break;
                case MouseDownMode.ResizeTop:
                    DesiredHeight = Math.Max(_mouseDownHeight - (int)dist.Value.Y, 0);
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left,
                        _mouseDownMargin.Value.Top + (int)dist.Value.Y, 0, 0);
                    break;
                case MouseDownMode.ResizeTopRight:
                    DesiredHeight = Math.Max(_mouseDownHeight - (int)dist.Value.Y, 0);
                    DesiredWidth = Math.Max(_mouseDownWidth + (int)dist.Value.X, 0);
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left,
                        _mouseDownMargin.Value.Top + (int)dist.Value.Y, 0, 0);
                    break;
                case MouseDownMode.ResizeRight:
                    DesiredWidth = Math.Max(_mouseDownWidth + (int)dist.Value.X, 0);
                    break;
                case MouseDownMode.ResizeBottomRight:
                    DesiredWidth = Math.Max(_mouseDownWidth + (int)dist.Value.X, 0);
                    DesiredHeight = Math.Max(_mouseDownHeight + (int)dist.Value.Y, 0);
                    break;
                case MouseDownMode.ResizeBottom:
                    DesiredHeight = Math.Max(_mouseDownHeight + (int)dist.Value.Y, 0);
                    break;
                case MouseDownMode.ResizeBottomLeft:
                    DesiredWidth = Math.Max(_mouseDownWidth - (int)dist.Value.X, 0);
                    DesiredHeight = Math.Max(_mouseDownHeight + (int)dist.Value.Y, 0);
                    DesiredMargin = new Thickness(_mouseDownMargin.Value.Left + (int)dist.Value.X,
                        _mouseDownMargin.Value.Top, 0, 0);
                    break;
            }
        }
    }

    private void MouseHost_OnMouseLeave(object sender, MouseEventArgs e)
    {
        DetachFromGlobalMouseHost(sender);
    }

    private void MouseHost_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        DetachFromGlobalMouseHost(sender);
    }

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class HeaderMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bVal && bVal)
                return new Thickness(0);
            return new Thickness(0, 5, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private enum MouseDownMode
    {
        DragHeader,
        ResizeLeft,
        ResizeTopLeft,
        ResizeTop,
        ResizeTopRight,
        ResizeRight,
        ResizeBottomRight,
        ResizeBottom,
        ResizeBottomLeft
    }
}