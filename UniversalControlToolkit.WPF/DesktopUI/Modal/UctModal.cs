using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UniversalControlToolkit.WPF.DesktopUI.Utils;

namespace UniversalControlToolkit.WPF.DesktopUI.Modal;

[TemplatePart(Name = "PART_ModalBackground", Type = typeof(Border))]
[ContentProperty(nameof(Content))]
public class UctModal : Control
{
    //--------------------------
    //
    //      header
    //
    //--------------------------

    private static readonly Brush _modalBackgroundBrush;

    private readonly Border _brdModalWindow;
    private Border? _partModalBackground;
    private bool _needsResize = false;
    private bool _hasFadedOut = true;
    private bool _hasOriginalSize = true;

    private MouseDownMode? _mouseDownMode;
    private Point? _mouseDownPosition;
    private double _mouseDownLeftMargin, _mouseDownTopMargin, _mouseDownWidth, _mouseDownHeight;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    static UctModal()
    {
        _modalBackgroundBrush = new SolidColorBrush(Color.FromArgb(0x80, 0x80, 0x80, 0x80));
        _modalBackgroundBrush.Freeze();
        VisibilityProperty.OverrideMetadata(typeof(UctModal),
            new FrameworkPropertyMetadata(Visibility.Collapsed, VisibilityChanged, CoerceVisibility));
        FrameworkElementFactory feModal = new FrameworkElementFactory(typeof(Border), "PART_ModalBackground");
        ControlTemplate ct = new ControlTemplate(typeof(UctModal)) { VisualTree = feModal };
        ct.Seal();
        TemplateProperty.OverrideMetadata(typeof(UctModal), new FrameworkPropertyMetadata(ct));
    }

    public UctModal()
    {
        Grid grdContent = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            RowDefinitions =
            {
                new RowDefinition() { Height = GridLength.Auto }, // Header row
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) } // Content row
            },
            ColumnDefinitions =
            {
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }, // Title column
                new ColumnDefinition() { Width = GridLength.Auto }, // Close button
            }
        };
        TextBlock tbTitle = new TextBlock()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        tbTitle.SetBinding(TextBlock.TextProperty, new Binding(nameof(Title)) { Source = this });
        grdContent.Children.Add(tbTitle);

        var btnClose = new UctImageButton()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center
        };
        btnClose.SetBinding(UctImageButton.ContentTemplateProperty,
            new Binding(nameof(UctVirtualDesktop.CloseButtonTemplate)) { Source = UctVirtualDesktop._currentDesktop });
        Grid.SetColumn(btnClose, 1);
        btnClose.MouseLeftButtonDown += (sender, args) => Visibility = Visibility.Collapsed;
        grdContent.Children.Add(btnClose);

        ContentPresenter cp = new ContentPresenter();
        cp.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(Content)) { Source = this });
        Border brdContentBackground = new Border()
        {
            Margin = new Thickness(5),
            Child = cp
        };
        brdContentBackground.SetBinding(Border.BackgroundProperty,
            new Binding(nameof(ContentBackground)) { Source = this });
        Grid.SetColumnSpan(brdContentBackground, 2);
        Grid.SetRow(brdContentBackground, 1);
        grdContent.Children.Add(brdContentBackground);

        _brdModalWindow = new Border()
        {
            Child = grdContent,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        _brdModalWindow.SetBinding(Border.BackgroundProperty, new Binding(nameof(WindowBackground)) { Source = this });
        _brdModalWindow.MouseMove += BrdModalWindowOnMouseMove;
        _brdModalWindow.MouseDown += BrdModalWindowOnMouseDown;
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    public Brush? ContentBackground
    {
        get => (Brush?)GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(UctModal),
            new PropertyMetadata(null));

    public Brush? WindowBackground
    {
        get => (Brush?)GetValue(WindowBackgroundProperty);
        set => SetValue(WindowBackgroundProperty, value);
    }

    public static readonly DependencyProperty WindowBackgroundProperty =
        DependencyProperty.Register(nameof(WindowBackground), typeof(Brush), typeof(UctModal),
            new PropertyMetadata(null));

    public double DesiredHeight
    {
        get => (double)GetValue(DesiredHeightProperty);
        set => SetValue(DesiredHeightProperty, value);
    }

    public static readonly DependencyProperty DesiredHeightProperty =
        DependencyProperty.Register(nameof(DesiredHeight), typeof(double), typeof(UctModal),
            new PropertyMetadata(double.NaN));

    public double DesiredWidth
    {
        get => (double)GetValue(DesiredWidthProperty);
        set => SetValue(DesiredWidthProperty, value);
    }

    public static readonly DependencyProperty DesiredWidthProperty =
        DependencyProperty.Register(nameof(DesiredWidth), typeof(double), typeof(UctModal),
            new PropertyMetadata(double.NaN));

    public double DesiredSizeRatio
    {
        get => (double)GetValue(DesiredSizeRatioProperty);
        set => SetValue(DesiredSizeRatioProperty, value);
    }

    public static readonly DependencyProperty DesiredSizeRatioProperty =
        DependencyProperty.Register(nameof(DesiredSizeRatio), typeof(double), typeof(UctModal),
            new PropertyMetadata(0.75));

    public object? Content
    {
        get => (object?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctModal), new PropertyMetadata(null));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(UctModal), new PropertyMetadata("Title"));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_partModalBackground != null)
        {
            _partModalBackground.Child = null;
            BindingOperations.ClearBinding(_partModalBackground, Border.BackgroundProperty);
            _partModalBackground.SizeChanged -= OnSizeChanged;
        }

        _partModalBackground = GetTemplateChild("PART_ModalBackground") as Border;

        if (_partModalBackground != null)
        {
            _partModalBackground.Child = _brdModalWindow;
            _partModalBackground.IsHitTestVisible = true;
            _partModalBackground.SetBinding(Border.BackgroundProperty,
                new Binding(nameof(Background)) { Source = this });
            _partModalBackground.SizeChanged += OnSizeChanged;
            if (Visibility == Visibility.Visible)
                ShowModal();
        }

        _needsResize = ResizeAndCenterModalWindow();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs sizeInfo)
    {
        if (_needsResize && sizeInfo.NewSize.Height > 0 && sizeInfo.NewSize.Width > 0)
        {
            _needsResize = ResizeAndCenterModalWindow();
        }
    }

    private void ShowModal()
    {
        if (_partModalBackground == null)
            return;
        _partModalBackground.Opacity = 0;
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.5),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
        };
        _hasOriginalSize = true;
        _needsResize = ResizeAndCenterModalWindow();
        _hasFadedOut = false;
        _partModalBackground.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }

    private void HideModal()
    {
        if (_partModalBackground == null)
            return;
        var fadeIn = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.5),
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
        };
        fadeIn.Completed += (sender, args) =>
        {
            _hasFadedOut = true;
            Visibility = Visibility.Collapsed;
        };
        _partModalBackground.BeginAnimation(UIElement.OpacityProperty, fadeIn);
    }

    /// <summary>
    /// Resizes the modal window based on the content size or desired dimensions.
    /// </summary>
    /// <returns>true, if another resize is necessary, otherwise returns false</returns>
    private bool ResizeAndCenterModalWindow()
    {
        if (ActualHeight <= 0 || ActualWidth <= 0)
            return true;
        double width = DesiredWidth;
        double height = DesiredHeight;

        if (double.IsNaN(width) || double.IsNaN(height))
        {
            width = ActualWidth * DesiredSizeRatio;
            height = ActualHeight * DesiredSizeRatio;
        }

        _brdModalWindow.Width = width;
        _brdModalWindow.Height = height;
        _brdModalWindow.Margin = new Thickness((ActualWidth - width) / 2, (ActualHeight - height) / 2, 0, 0);
        return _hasOriginalSize;
    }

    //--------------------------
    //
    //      events
    //
    //--------------------------

    private static void VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UctModal modal && e.NewValue is Visibility visibility)
        {
            if (visibility == Visibility.Visible)
            {
                modal.ShowModal();
            }
            else
            {
                modal.HideModal();
            }
        }
    }

    private static object CoerceVisibility(DependencyObject d, object baseValue)
    {
        if (d is UctModal modal && baseValue is Visibility visibility)
        {
            if (visibility == Visibility.Collapsed && !modal._hasFadedOut)
            {
                modal.HideModal();
                return System.Windows.Visibility.Visible;
            }

            return visibility;
        }

        return baseValue;
    }

    private void BrdModalWindowOnMouseMove(object sender, MouseEventArgs e)
    {
        if (_partModalBackground == null)
            return;
        var pos = e.GetPosition(_brdModalWindow);
        bool isLeft = pos.X <= 5;
        bool isRight = pos.X >= _brdModalWindow.ActualWidth - 5;
        bool isTop = pos.Y <= 5;
        bool isBottom = pos.Y >= _brdModalWindow.ActualHeight - 5;
        Cursor usedCursor;
        if (isTop)
        {
            if (isLeft)
                usedCursor = Cursors.SizeNWSE;
            else if (isRight)
                usedCursor = Cursors.SizeNESW;
            else
                usedCursor = Cursors.SizeNS;
        }
        else if (isBottom)
        {
            if (isLeft)
                usedCursor = Cursors.SizeNESW;
            else if (isRight)
                usedCursor = Cursors.SizeNWSE;
            else
                usedCursor = Cursors.SizeNS;
        }
        else if (isLeft)
        {
            usedCursor = Cursors.SizeWE;
        }
        else if (isRight)
        {
            usedCursor = Cursors.SizeWE;
        }
        else
        {
            usedCursor = Cursors.Arrow;
        }

        _brdModalWindow.Cursor = usedCursor;
    }

    private void BrdModalWindowOnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_partModalBackground == null)
            return;
        var pos = e.GetPosition(_brdModalWindow);
        bool isLeft = pos.X <= 5;
        bool isRight = pos.X >= _brdModalWindow.ActualWidth - 5;
        bool isTop = pos.Y <= 5;
        bool isBottom = pos.Y >= _brdModalWindow.ActualHeight - 5;
        MouseDownMode mouseDownMode;
        if (isTop)
        {
            if (isLeft)
                mouseDownMode = MouseDownMode.ResizeTopLeft;
            else if (isRight)
                mouseDownMode = MouseDownMode.ResizeTopRight;
            else
                mouseDownMode = MouseDownMode.ResizeTop;
        }
        else if (isBottom)
        {
            if (isLeft)
                mouseDownMode = MouseDownMode.ResizeBottomLeft;
            else if (isRight)
                mouseDownMode = MouseDownMode.ResizeBottomRight;
            else
                mouseDownMode = MouseDownMode.ResizeBottom;
        }
        else if (isLeft)
        {
            mouseDownMode = MouseDownMode.ResizeLeft;
        }
        else if (isRight)
        {
            mouseDownMode = MouseDownMode.ResizeRight;
        }
        else if (pos.Y < 25)
        {
            mouseDownMode = MouseDownMode.DragHeader;
        }
        else
        {
            return;
        }

        _mouseDownLeftMargin = _brdModalWindow.Margin.Left;
        _mouseDownTopMargin = _brdModalWindow.Margin.Top;
        _mouseDownWidth = _brdModalWindow.Width;
        _mouseDownHeight = _brdModalWindow.Height;
        _mouseDownMode = mouseDownMode;
        _mouseDownPosition = e.GetPosition(_partModalBackground);
        _partModalBackground.Cursor = _brdModalWindow.Cursor;
        _partModalBackground.MouseMove += BrdModalBackgroundOnMouseMove;
        _brdModalWindow.IsHitTestVisible = false;
        _partModalBackground.MouseLeave += BrdModalBackgroundOnMouseLeave;
        _partModalBackground.MouseUp += BrdModalBackgroundOnMouseUp;
    }

    private void BrdModalBackgroundOnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseDownMode.HasValue || !_mouseDownPosition.HasValue || _partModalBackground == null)
            return;
        var diff = e.GetPosition(_partModalBackground) - _mouseDownPosition.Value;
        switch (_mouseDownMode.Value)
        {
            case MouseDownMode.DragHeader:
                _brdModalWindow.Margin =
                    new Thickness(
                        Math.Clamp(_mouseDownLeftMargin + diff.X, 0,
                            _partModalBackground.ActualWidth - _mouseDownWidth),
                        Math.Clamp(_mouseDownTopMargin + diff.Y, 0,
                            _partModalBackground.ActualHeight - _mouseDownHeight), 0, 0);
                break;
            case MouseDownMode.ResizeLeft:
                _brdModalWindow.Margin =
                    new Thickness(Math.Min(_mouseDownLeftMargin + diff.X, _mouseDownLeftMargin + _mouseDownWidth - 100),
                        _mouseDownTopMargin, 0, 0);
                _brdModalWindow.Width = Math.Max(_mouseDownWidth - diff.X, 100);
                break;
            case MouseDownMode.ResizeTopLeft:
                _brdModalWindow.Margin =
                    new Thickness(Math.Min(_mouseDownLeftMargin + diff.X, _mouseDownLeftMargin + _mouseDownWidth - 100),
                        Math.Min(_mouseDownTopMargin + diff.Y, _mouseDownTopMargin + _mouseDownHeight - 100), 0, 0);
                _brdModalWindow.Width = Math.Max(_mouseDownWidth - diff.X, 100);
                _brdModalWindow.Height = Math.Max(_mouseDownHeight - diff.Y, 100);
                break;
            case MouseDownMode.ResizeTop:
                _brdModalWindow.Margin =
                    new Thickness(_mouseDownLeftMargin,
                        Math.Min(_mouseDownTopMargin + diff.Y, _mouseDownTopMargin + _mouseDownHeight - 100), 0, 0);
                _brdModalWindow.Height = Math.Max(_mouseDownHeight - diff.Y, 100);
                break;
            case MouseDownMode.ResizeTopRight:
                _brdModalWindow.Margin =
                    new Thickness(_mouseDownLeftMargin,
                        Math.Min(_mouseDownTopMargin + diff.Y, _mouseDownTopMargin + _mouseDownHeight - 100), 0, 0);
                _brdModalWindow.Width = Math.Max(_mouseDownWidth + diff.X, 100);
                _brdModalWindow.Height = Math.Max(_mouseDownHeight - diff.Y, 100);
                break;
            case MouseDownMode.ResizeRight:
                _brdModalWindow.Width = Math.Max(_mouseDownWidth + diff.X, 100);
                break;
            case MouseDownMode.ResizeBottomRight:
                _brdModalWindow.Width = Math.Max(_mouseDownWidth + diff.X, 100);
                _brdModalWindow.Height = Math.Max(_mouseDownHeight + diff.Y, 100);
                break;
            case MouseDownMode.ResizeBottom:
                _brdModalWindow.Height = Math.Max(_mouseDownHeight + diff.Y, 100);
                break;
            case MouseDownMode.ResizeBottomLeft:
                _brdModalWindow.Margin =
                    new Thickness(Math.Min(_mouseDownLeftMargin + diff.X, _mouseDownLeftMargin + _mouseDownWidth - 100),
                        _mouseDownTopMargin, 0, 0);
                _brdModalWindow.Width = Math.Max(_mouseDownWidth - diff.X, 100);
                _brdModalWindow.Height = Math.Max(_mouseDownHeight + diff.Y, 100);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _hasOriginalSize = false;
        _needsResize = false;
    }

    private void BrdModalBackgroundOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_partModalBackground == null)
            return;
        _mouseDownMode = null;
        _mouseDownPosition = null;
        _brdModalWindow.IsHitTestVisible = true;
        _partModalBackground.Cursor = Cursors.Arrow;
    }

    private void BrdModalBackgroundOnMouseLeave(object sender, MouseEventArgs e)
    {
        if (_partModalBackground == null)
            return;
        _mouseDownMode = null;
        _mouseDownPosition = null;
        _brdModalWindow.IsHitTestVisible = true;
        _partModalBackground.Cursor = Cursors.Arrow;
    }

    //--------------------------
    //
    //      classes
    //
    //--------------------------

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