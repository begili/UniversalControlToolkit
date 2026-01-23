using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UniversalControlToolkit.WPF.DesktopUI.Modal;

[ContentProperty(nameof(Content))]
public class UctFlyIn : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    private readonly Border _border;
    private readonly ContentPresenter _contentPresenter;
    private readonly UctImageButton _closeButton;
    private readonly Storyboard _fadeInStoryboard;
    private readonly Storyboard _fadeOutStoryboard;
    private bool _hasFadedOut = true;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    static UctFlyIn()
    {
        VisibilityProperty.OverrideMetadata(typeof(UctFlyIn),
            new FrameworkPropertyMetadata(Visibility.Collapsed, VisibilityChanged, CoerceVisibility));
    }

    public UctFlyIn()
    {
        // Create grid with header row and content row (like UctModal)
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

        // Create title text block
        TextBlock tbTitle = new TextBlock()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        tbTitle.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(nameof(Title)) { Source = this });
        grdContent.Children.Add(tbTitle);

        // Create close button using UctImageButton like UctModal
        _closeButton = new UctImageButton()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center
        };
        _closeButton.SetBinding(UctImageButton.ContentTemplateProperty,
            new System.Windows.Data.Binding(nameof(UctVirtualDesktop.CloseButtonTemplate)) 
            { 
                Source = UctVirtualDesktop._currentDesktop 
            });
        Grid.SetColumn(_closeButton, 1);
        _closeButton.MouseLeftButtonDown += (_, _) => Visibility = Visibility.Collapsed;
        _closeButton.SetBinding(VisibilityProperty, 
            new System.Windows.Data.Binding(nameof(ShowCloseButton)) 
            { 
                Source = this, 
                Converter = new BooleanToVisibilityConverter() 
            });
        grdContent.Children.Add(_closeButton);

        // Create content presenter for child content
        _contentPresenter = new ContentPresenter();
        _contentPresenter.SetBinding(ContentPresenter.ContentProperty, 
            new System.Windows.Data.Binding(nameof(Content)) { Source = this });
        _contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, 
            new System.Windows.Data.Binding(nameof(ContentTemplate)) { Source = this });

        // Wrap content presenter in a border with ContentBackground
        Border brdContentBackground = new Border()
        {
            Margin = new Thickness(5),
            Child = _contentPresenter
        };
        brdContentBackground.SetBinding(Border.BackgroundProperty,
            new System.Windows.Data.Binding(nameof(ContentBackground)) { Source = this });
        brdContentBackground.SetBinding(Border.CornerRadiusProperty,
            new System.Windows.Data.Binding(nameof(CornerRadius)) { Source = this });
        Grid.SetColumnSpan(brdContentBackground, 2);
        Grid.SetRow(brdContentBackground, 1);
        grdContent.Children.Add(brdContentBackground);

        // Create border
        _border = new Border
        {
            Child = grdContent,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Gray,
            Background = Brushes.White,
            Padding = new Thickness(8)
        };
        _border.SetBinding(Border.BorderBrushProperty, 
            new System.Windows.Data.Binding(nameof(BorderBrush)) { Source = this });
        _border.SetBinding(Border.BorderThicknessProperty, 
            new System.Windows.Data.Binding(nameof(BorderThickness)) { Source = this });
        _border.SetBinding(Border.BackgroundProperty, 
            new System.Windows.Data.Binding(nameof(Background)) { Source = this });
        _border.SetBinding(Border.PaddingProperty, 
            new System.Windows.Data.Binding(nameof(Padding)) { Source = this });
        _border.SetBinding(Border.CornerRadiusProperty, 
            new System.Windows.Data.Binding(nameof(CornerRadius)) { Source = this });

        // Create fade-in animation
        _fadeInStoryboard = new Storyboard();
        var fadeInAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(FadeInDuration)
        };
        Storyboard.SetTarget(fadeInAnimation, _border);
        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));
        _fadeInStoryboard.Children.Add(fadeInAnimation);

        // Create fade-out animation
        _fadeOutStoryboard = new Storyboard();
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(FadeOutDuration)
        };
        Storyboard.SetTarget(fadeOutAnimation, _border);
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));
        _fadeOutStoryboard.Children.Add(fadeOutAnimation);
        _fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;

        // Add border as visual child
        AddVisualChild(_border);

        // Set initial opacity
        _border.Opacity = 0;
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    protected override int VisualChildrenCount => 1;

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctFlyIn),
            new PropertyMetadata(null, ContentChanged));

    public Brush? ContentBackground
    {
        get => (Brush?)GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(UctFlyIn),
            new PropertyMetadata(null));

    private static void ContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UctFlyIn flyIn)
        {
            flyIn._contentPresenter.Content = e.NewValue;
            
            if (e.OldValue != null)
                flyIn.RemoveLogicalChild(e.OldValue);
            if (e.NewValue != null)
                flyIn.AddLogicalChild(e.NewValue);
        }
    }

    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(nameof(ContentTemplate), typeof(DataTemplate), typeof(UctFlyIn),
            new PropertyMetadata(null));

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(UctFlyIn),
            new PropertyMetadata(true));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(UctFlyIn), 
            new PropertyMetadata(""));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(UctFlyIn),
            new PropertyMetadata(new CornerRadius(4)));

    public int FadeInDuration
    {
        get => (int)GetValue(FadeInDurationProperty);
        set => SetValue(FadeInDurationProperty, value);
    }

    public static readonly DependencyProperty FadeInDurationProperty =
        DependencyProperty.Register(nameof(FadeInDuration), typeof(int), typeof(UctFlyIn),
            new PropertyMetadata(300));

    public int FadeOutDuration
    {
        get => (int)GetValue(FadeOutDurationProperty);
        set => SetValue(FadeOutDurationProperty, value);
    }

    public static readonly DependencyProperty FadeOutDurationProperty =
        DependencyProperty.Register(nameof(FadeOutDuration), typeof(int), typeof(UctFlyIn),
            new PropertyMetadata(200));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index)
    {
        if (index != 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _border;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _border.Measure(constraint);
        return _border.DesiredSize;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        _border.Arrange(new Rect(arrangeBounds));
        return arrangeBounds;
    }

    private static void VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UctFlyIn flyIn && e.NewValue is Visibility visibility)
        {
            if (visibility == Visibility.Visible)
            {
                flyIn.ShowFlyIn();
            }
            else
            {
                flyIn.HideFlyIn();
            }
        }
    }

    private static object CoerceVisibility(DependencyObject d, object baseValue)
    {
        if (d is UctFlyIn flyIn && baseValue is Visibility visibility)
        {
            if (visibility == Visibility.Collapsed && !flyIn._hasFadedOut)
            {
                flyIn.HideFlyIn();
                return Visibility.Visible;
            }

            return visibility;
        }

        return baseValue;
    }

    private void ShowFlyIn()
    {
        _fadeOutStoryboard.Stop();
        _border.Opacity = 0;
        _hasFadedOut = false;
        _fadeInStoryboard.Begin();
    }

    private void HideFlyIn()
    {
        _fadeInStoryboard.Stop();
        if (_border.Opacity > 0)
        {
            _fadeOutStoryboard.Begin();
        }
        else
        {
            _hasFadedOut = true;
            Visibility = Visibility.Collapsed;
        }
    }

    private void FadeOutStoryboard_Completed(object? sender, EventArgs e)
    {
        // Ensure fully transparent after fade out
        _border.Opacity = 0;
        _hasFadedOut = true;
        Visibility = Visibility.Collapsed;
    }

    public void Show()
    {
        Visibility = Visibility.Visible;
    }

    public void Close()
    {
        Visibility = Visibility.Collapsed;
    }
}