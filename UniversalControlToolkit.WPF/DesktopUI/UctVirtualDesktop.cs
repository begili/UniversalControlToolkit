using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctVirtualDesktop : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    private readonly Grid _gridHost, _gridTaskbar;
    private readonly ContentPresenter _cpStartButton;
    private readonly StackPanel _applicationPanel;
    private readonly Border _brdModal, _brdTaskbar, _brdTaskbarMenu, _brdContent, _brdStartButton;
    private readonly UctMenu _taskbarMenu;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctVirtualDesktop()
    {
        ColumnDefinition cDefRight;
        ColumnDefinition cDefLeft;
        RowDefinition rDefBottom;
        RowDefinition rDefTop;
        _gridHost = new Grid()
        {
            RowDefinitions =
            {
                (rDefTop = new RowDefinition()),
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
                (rDefBottom = new RowDefinition())
            },
            ColumnDefinitions =
            {
                (cDefLeft = new ColumnDefinition()),
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) },
                (cDefRight = new ColumnDefinition())
            }
        };

        RcDefConverter converter = new RcDefConverter();
        rDefTop.SetBinding(RowDefinition.HeightProperty, new MultiBinding()
        {
            Converter = converter,
            Bindings =
            {
                new Binding(nameof(TaskbarSize)) { Source = this },
                new Binding(nameof(TaskbarPlacement)) { Source = this }
            },
            ConverterParameter = (int)TaskbarPlacement.Top
        });
        rDefBottom.SetBinding(RowDefinition.HeightProperty, new MultiBinding()
        {
            Converter = converter,
            Bindings =
            {
                new Binding(nameof(TaskbarSize)) { Source = this },
                new Binding(nameof(TaskbarPlacement)) { Source = this }
            },
            ConverterParameter = (int)TaskbarPlacement.Bottom
        });
        cDefLeft.SetBinding(ColumnDefinition.WidthProperty, new MultiBinding()
        {
            Converter = converter,
            Bindings =
            {
                new Binding(nameof(TaskbarSize)) { Source = this },
                new Binding(nameof(TaskbarPlacement)) { Source = this }
            },
            ConverterParameter = (int)TaskbarPlacement.Left
        });
        cDefRight.SetBinding(ColumnDefinition.WidthProperty, new MultiBinding()
        {
            Converter = converter,
            Bindings =
            {
                new Binding(nameof(TaskbarSize)) { Source = this },
                new Binding(nameof(TaskbarPlacement)) { Source = this }
            },
            ConverterParameter = (int)TaskbarPlacement.Right
        });

        _brdContent = new Border();
        Grid.SetRow(_brdContent, 1);
        Grid.SetColumn(_brdContent, 1);
        _gridHost.Children.Add(_brdContent);

        _gridTaskbar = new Grid();

        _applicationPanel = new StackPanel() { Background = Brushes.DarkGray };
        Grid.SetColumn(_applicationPanel, 1);
        Grid.SetRow(_applicationPanel, 1);
        _gridTaskbar.Children.Add(_applicationPanel);

        _cpStartButton = new ContentPresenter();
        _cpStartButton.SetBinding(ContentPresenter.ContentProperty,
            new Binding(nameof(StartButtonContent)) { Source = this });

        var viewBox = new Viewbox()
        {
            Child = _cpStartButton, VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        _brdStartButton = new Border() { Background = Brushes.Transparent, Child = viewBox };
        _brdStartButton.SetBinding(Border.HeightProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        _brdStartButton.SetBinding(Border.WidthProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        _brdStartButton.MouseLeftButtonDown += CpStartButton_OnMouseLeftButtonDown;
        _gridTaskbar.Children.Add(_brdStartButton);

        _brdTaskbar = new Border()
        {
            Child = _gridTaskbar, Background = Brushes.DimGray
        };

        _gridHost.Children.Add(_brdTaskbar);

        _brdModal = new Border()
        {
            IsHitTestVisible = true,
            Background = Brushes.Transparent,
            Visibility = Visibility.Collapsed
        };
        Grid.SetRowSpan(_brdModal, 3);
        Grid.SetColumnSpan(_brdModal, 3);
        _brdModal.MouseDown += BrdModal_OnMouseDown;
        _gridHost.Children.Add(_brdModal);

        _taskbarMenu = new UctMenu();
        _taskbarMenu.ModuleDefinitionClicked += TaskbarMenu_OnModuleDefinitionClicked;
        ScrollViewer scrv = new ScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _taskbarMenu
        };

        _brdTaskbarMenu = new Border()
        {
            Visibility = Visibility.Collapsed,
            Background = Brushes.Gray,
            Child = scrv
        };
        Grid.SetRow(_brdTaskbarMenu, 1);
        Grid.SetColumn(_brdTaskbarMenu, 1);
        _brdTaskbarMenu.SetBinding(UctMenu.WidthProperty, new Binding(nameof(TaskbarMenuWidth)) { Source = this });
        _brdTaskbarMenu.SetBinding(UctMenu.HeightProperty, new Binding(nameof(TaskbarMenuMaxHeight)) { Source = this });

        _gridHost.Children.Add(_brdTaskbarMenu);
        BuildupPlacementDependencies();
        AddVisualChild(_gridHost);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    protected override int VisualChildrenCount => 1;

    public double TaskbarSize
    {
        get => (double)GetValue(TaskbarSizeProperty);
        set => SetValue(TaskbarSizeProperty, value);
    }

    public static readonly DependencyProperty TaskbarSizeProperty =
        DependencyProperty.Register(nameof(TaskbarSize), typeof(double), typeof(UctVirtualDesktop),
            new PropertyMetadata(48.0));

    public TaskbarPlacement TaskbarPlacement
    {
        get => (TaskbarPlacement)GetValue(TaskbarPlacementProperty);
        set => SetValue(TaskbarPlacementProperty, value);
    }

    public static readonly DependencyProperty TaskbarPlacementProperty =
        DependencyProperty.Register(nameof(TaskbarPlacement), typeof(TaskbarPlacement), typeof(UctVirtualDesktop),
            new PropertyMetadata(TaskbarPlacement.Bottom, OnTaskbarPlacementChanged));

    private static void OnTaskbarPlacementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktop;
        snd?.BuildupPlacementDependencies();
    }

    public double TaskbarMenuMaxHeight
    {
        get => (double)GetValue(TaskbarMenuMaxHeightProperty);
        set => SetValue(TaskbarMenuMaxHeightProperty, value);
    }

    public static readonly DependencyProperty TaskbarMenuMaxHeightProperty =
        DependencyProperty.Register(nameof(TaskbarMenuMaxHeight), typeof(double), typeof(UctVirtualDesktop),
            new PropertyMetadata(650.0));

    public double TaskbarMenuWidth
    {
        get => (double)GetValue(TaskbarMenuWidthProperty);
        set => SetValue(TaskbarMenuWidthProperty, value);
    }

    public static readonly DependencyProperty TaskbarMenuWidthProperty =
        DependencyProperty.Register(nameof(TaskbarMenuWidth), typeof(double), typeof(UctVirtualDesktop),
            new PropertyMetadata(350.0));

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public CtkMenuItemCollection MenuItems => _taskbarMenu.Items;

    public object StartButtonContent
    {
        get => (object)GetValue(StartButtonContentProperty);
        set => SetValue(StartButtonContentProperty, value);
    }

    public static readonly DependencyProperty StartButtonContentProperty =
        DependencyProperty.Register(nameof(StartButtonContent), typeof(object), typeof(UctVirtualDesktop),
            new PropertyMetadata("Start"));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index)
    {
        return _gridHost;
    }

    private void BuildupPlacementDependencies()
    {
        switch (this.TaskbarPlacement)
        {
            case TaskbarPlacement.Bottom:
                Grid.SetRow(_brdTaskbar, 2);
                Grid.SetColumn(_brdTaskbar, 1);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Left;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case TaskbarPlacement.Top:
                Grid.SetRow(_brdTaskbar, 0);
                Grid.SetColumn(_brdTaskbar, 1);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Left;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                break;
            case TaskbarPlacement.Left:
                Grid.SetRow(_brdTaskbar, 1);
                Grid.SetColumn(_brdTaskbar, 0);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Left;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                break;
            case TaskbarPlacement.Right:
                Grid.SetRow(_brdTaskbar, 1);
                Grid.SetColumn(_brdTaskbar, 2);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Right;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                break;
        }

        _gridTaskbar.ColumnDefinitions.Clear();
        _gridTaskbar.RowDefinitions.Clear();
        switch (this.TaskbarPlacement)
        {
            case TaskbarPlacement.Top:
            case TaskbarPlacement.Bottom:
                _gridTaskbar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                _gridTaskbar.ColumnDefinitions.Add(new ColumnDefinition()
                    { Width = new GridLength(1, GridUnitType.Star) });
                _gridTaskbar.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                _applicationPanel.Margin = new Thickness(15, 0, 15, 0);
                _applicationPanel.Orientation = Orientation.Horizontal;
                break;
            case TaskbarPlacement.Left:
            case TaskbarPlacement.Right:
                _gridTaskbar.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                _gridTaskbar.RowDefinitions.Add(new RowDefinition()
                    { Height = new GridLength(1, GridUnitType.Star) });
                _gridTaskbar.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                _applicationPanel.Margin = new Thickness(0, 15, 0, 15);
                _applicationPanel.Orientation = Orientation.Vertical;
                break;
        }
    }

    //--------------------------
    //
    //      events
    //
    //--------------------------

    private void CpStartButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _brdModal.Visibility = _brdTaskbarMenu.Visibility =
            _brdTaskbarMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void BrdModal_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _brdModal.Visibility = _brdTaskbarMenu.Visibility = Visibility.Collapsed;
    }

    private void TaskbarMenu_OnModuleDefinitionClicked(object? sender, ModuleDefinitionClickedEventArgs e)
    {
        _brdContent.Child = e.UctModuleDefinition.GetModuleUI();
    }

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class RcDefConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is double size) ||
                !(values[1] is TaskbarPlacement tp))
                return GridLength.Auto;
            TaskbarPlacement type = (TaskbarPlacement)(parameter is int iVal ? iVal : 0); //TaskbarPlacement int values
            if (type == tp)
                return new GridLength(size);
            return GridLength.Auto;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

public enum TaskbarPlacement
{
    Bottom,
    Left,
    Top,
    Right
}