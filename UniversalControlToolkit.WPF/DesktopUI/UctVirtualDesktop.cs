using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;
using UniversalControlToolkit.WPF.DesktopUI.Utils;
using Brushes = System.Windows.Media.Brushes;

namespace UniversalControlToolkit.WPF.DesktopUI;

public class UctVirtualDesktop : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    private readonly Grid _gridHost, _gridTaskbar, _gridTaskbarMenu, _gridContent;
    private readonly ContentPresenter _cpStartButton;
    private readonly StackPanel _applicationPanel;

    private readonly Border _brdModal,
        _brdTaskbar,
        _brdTaskbarMenu,
        _brdContent,
        _brdStartButton,
        _brdGlobalMouseActions;

    private readonly UctMenu _taskbarMenu;
    private readonly DropShadowEffect _dseTaskbarMenu;
    private readonly UctVirtualDesktopPanel _desktopPanel;
    private readonly ContextMenu _ctxDesktopPanel;

    private readonly IList<RunningAppInfo> _runningAppInfos;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctVirtualDesktop()
    {
        _runningAppInfos = new List<RunningAppInfo>();

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

        MenuItem mnuBackground;
        _ctxDesktopPanel = new ContextMenu()
        {
            Items =
            {
                (mnuBackground = new MenuItem() { Header = "Hintergrund ändern" })
            }
        };
        mnuBackground.Click += MnuBackground_OnClick;

        _desktopPanel = new UctVirtualDesktopPanel()
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        if (CanUserChangeBackground)
            _desktopPanel.ContextMenu = _ctxDesktopPanel;
        _desktopPanel.SetBinding(UctVirtualDesktopPanel.BackgroundProperty,
            new Binding(nameof(DesktopBackground)) { Source = this });
        _desktopPanel.SetBinding(UctVirtualDesktopPanel.BackgroundImageProperty,
            new Binding(nameof(DesktopBackgroundImage)) { Source = this });

        _gridContent = new Grid();
        _gridContent.Children.Add(_desktopPanel);

        _brdContent = new Border()
        {
            Child = _gridContent
        };
        Grid.SetRow(_brdContent, 1);
        Grid.SetColumn(_brdContent, 1);
        _gridHost.Children.Add(_brdContent);

        _gridTaskbar = new Grid();

        _applicationPanel = new StackPanel();
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

        _brdStartButton = new Border() { Background = Brushes.Transparent, Child = viewBox, Margin = new Thickness(2) };
        _brdStartButton.SetBinding(Border.HeightProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        _brdStartButton.SetBinding(Border.WidthProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        _brdStartButton.MouseLeftButtonDown += CpStartButton_OnMouseLeftButtonDown;
        _gridTaskbar.Children.Add(_brdStartButton);

        _brdTaskbar = new Border()
        {
            Child = _gridTaskbar
        };
        _brdTaskbar.SetResourceReference(Border.BackgroundProperty, "UctTaskbarBackgroundColor");

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
        _taskbarMenu.SetResourceReference(UctMenu.ForegroundProperty, "UctTaskmenuForegroundColor");
        _taskbarMenu.SetResourceReference(UctMenu.HighlightBackgroundProperty, "UctTaskmenuHighlightedColor");
        _taskbarMenu.ModuleDefinitionClicked += TaskbarMenu_OnModuleDefinitionClicked;
        ScrollViewer scrv = new ScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _taskbarMenu
        };
        _taskbarMenu.SetBinding(UctMenu.GroupIconProperty, new Binding(nameof(GroupIcon)) { Source = this });

        _gridTaskbarMenu = new Grid()
        {
            RowDefinitions =
            {
                new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition() { Height = GridLength.Auto }
            }
        };
        _gridTaskbarMenu.Children.Add(scrv);
        var cpFooter = new ContentPresenter();
        cpFooter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(MenuFooterContent)) { Source = this });
        Grid.SetRow(cpFooter, 1);
        _gridTaskbarMenu.Children.Add(cpFooter);

        _brdTaskbarMenu = new Border()
        {
            Visibility = Visibility.Collapsed,
            Child = _gridTaskbarMenu,
            Effect = (_dseTaskbarMenu = new DropShadowEffect()
            {
                Color = Color.FromRgb(0x40, 0x40, 0x40),
                Opacity = 0.4
            })
        };
        _brdTaskbarMenu.SetResourceReference(Border.BackgroundProperty, "UctTaskmenuBackgroundColor");
        Grid.SetRow(_brdTaskbarMenu, 1);
        Grid.SetColumn(_brdTaskbarMenu, 1);
        _brdTaskbarMenu.SetBinding(UctMenu.WidthProperty, new Binding(nameof(TaskbarMenuWidth)) { Source = this });
        _brdTaskbarMenu.SetBinding(UctMenu.HeightProperty, new Binding(nameof(TaskbarMenuMaxHeight)) { Source = this });

        _gridHost.Children.Add(_brdTaskbarMenu);

        _brdGlobalMouseActions = new Border()
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent
        };
        Grid.SetColumnSpan(_brdGlobalMouseActions, 3);
        Grid.SetRowSpan(_brdGlobalMouseActions, 3);
        _gridHost.Children.Add(_brdGlobalMouseActions);
        BuildupPlacementDependencies();
        AddVisualChild(_gridHost);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    public static readonly DependencyProperty AppInfoProperty =
        DependencyProperty.RegisterAttached("AppInfo", typeof(RunningAppInfo), typeof(UctVirtualDesktop),
            new PropertyMetadata(defaultValue: null));

    public static RunningAppInfo GetAppInfo(UctVirtualDesktopApplicationPanel target) =>
        (RunningAppInfo)target.GetValue(AppInfoProperty);

    public static void SetAppInfo(UctVirtualDesktopApplicationPanel target, RunningAppInfo value) =>
        target.SetValue(AppInfoProperty, value);

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

    public DataTemplate DefaultAppIcon
    {
        get => (DataTemplate)GetValue(DefaultAppIconProperty);
        set => SetValue(DefaultAppIconProperty, value);
    }

    public static readonly DependencyProperty DefaultAppIconProperty =
        DependencyProperty.Register(nameof(DefaultAppIcon), typeof(DataTemplate), typeof(UctVirtualDesktop),
            new PropertyMetadata(null));

    public DataTemplate GroupIcon
    {
        get => (DataTemplate)GetValue(GroupIconProperty);
        set => SetValue(GroupIconProperty, value);
    }

    public static readonly DependencyProperty GroupIconProperty =
        DependencyProperty.Register(nameof(GroupIcon), typeof(DataTemplate), typeof(UctVirtualDesktop),
            new PropertyMetadata(null));

    public object MenuFooterContent
    {
        get => (object)GetValue(MenuFooterContentProperty);
        set => SetValue(MenuFooterContentProperty, value);
    }

    public static readonly DependencyProperty MenuFooterContentProperty =
        DependencyProperty.Register(nameof(MenuFooterContent), typeof(object), typeof(UctVirtualDesktop),
            new PropertyMetadata(null));

    public Brush DesktopBackground
    {
        get => (Brush)GetValue(DesktopBackgroundProperty);
        set => SetValue(DesktopBackgroundProperty, value);
    }

    public static readonly DependencyProperty DesktopBackgroundProperty =
        DependencyProperty.Register(nameof(DesktopBackground), typeof(Brush), typeof(UctVirtualDesktop),
            new PropertyMetadata(Brushes.Transparent));

    public string DesktopBackgroundImage
    {
        get => (string)GetValue(DesktopBackgroundImageProperty);
        set => SetValue(DesktopBackgroundImageProperty, value);
    }

    public static readonly DependencyProperty DesktopBackgroundImageProperty =
        DependencyProperty.Register(nameof(DesktopBackgroundImage), typeof(string), typeof(UctVirtualDesktop),
            new PropertyMetadata(null));

    public bool CanUserChangeBackground
    {
        get => (bool)GetValue(CanUserChangeBackgroundProperty);
        set => SetValue(CanUserChangeBackgroundProperty, value);
    }

    public static readonly DependencyProperty CanUserChangeBackgroundProperty =
        DependencyProperty.Register(nameof(CanUserChangeBackground), typeof(bool), typeof(UctVirtualDesktop),
            new PropertyMetadata(true, OnCanUserChangeBackgroundChanged));

    private static void OnCanUserChangeBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var snd = sender as UctVirtualDesktop;
        var newValue = (bool)e.NewValue;
        snd._desktopPanel.ContextMenu = newValue ? snd._ctxDesktopPanel : null;
    }

    public DataTemplate CloseButtonTemplate
    {
        get => (DataTemplate)GetValue(CloseButtonTemplateProperty);
        set => SetValue(CloseButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty CloseButtonTemplateProperty =
        DependencyProperty.Register(nameof(CloseButtonTemplate), typeof(DataTemplate), typeof(UctVirtualDesktop),
            new PropertyMetadata(null));

    public DataTemplate MaximizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MaximizedButtonTemplateProperty);
        set => SetValue(MaximizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MaximizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MaximizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktop), new PropertyMetadata(null));

    public DataTemplate MinimizedButtonTemplate
    {
        get => (DataTemplate)GetValue(MinimizedButtonTemplateProperty);
        set => SetValue(MinimizedButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty MinimizedButtonTemplateProperty =
        DependencyProperty.Register(nameof(MinimizedButtonTemplate), typeof(DataTemplate),
            typeof(UctVirtualDesktop), new PropertyMetadata(null));

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
                _dseTaskbarMenu.Direction = 45;
                break;
            case TaskbarPlacement.Top:
                Grid.SetRow(_brdTaskbar, 0);
                Grid.SetColumn(_brdTaskbar, 1);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Left;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                _dseTaskbarMenu.Direction = 315;
                break;
            case TaskbarPlacement.Left:
                Grid.SetRow(_brdTaskbar, 1);
                Grid.SetColumn(_brdTaskbar, 0);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Left;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                _dseTaskbarMenu.Direction = 315;
                break;
            case TaskbarPlacement.Right:
                Grid.SetRow(_brdTaskbar, 1);
                Grid.SetColumn(_brdTaskbar, 2);
                _brdTaskbarMenu.HorizontalAlignment = HorizontalAlignment.Right;
                _brdTaskbarMenu.VerticalAlignment = VerticalAlignment.Top;
                _dseTaskbarMenu.Direction = 225;
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

    private void LaunchOrResumeApplication(RunningAppInfo appInfo)
    {
        bool isNew = false;
        if (!_runningAppInfos.Contains(appInfo))
        {
            _runningAppInfos.Add(appInfo);
            _applicationPanel.Children.Add(appInfo.AppButton);
            isNew = true;
        }

        if (isNew)
        {
            UctVirtualDesktopApplicationPanel appPanel = new UctVirtualDesktopApplicationPanel()
            {
                Content = appInfo.InstancedUI
            };
            appPanel.SetResourceReference(UctVirtualDesktopApplicationPanel.BackgroundProperty,
                "UctWindowBackgroundColor");
            appPanel.SetResourceReference(UctVirtualDesktopApplicationPanel.ContentBackgroundProperty,
                "UctDefaultBackgroundColor");
            appPanel.SetBinding(UctVirtualDesktopApplicationPanel.MinimizedButtonTemplateProperty,
                new Binding(nameof(MinimizedButtonTemplate)) { Source = this });
            appPanel.SetBinding(UctVirtualDesktopApplicationPanel.MaximizedButtonTemplateProperty,
                new Binding(nameof(MaximizedButtonTemplate)) { Source = this });
            appPanel.SetBinding(UctVirtualDesktopApplicationPanel.CloseButtonTemplateProperty,
                new Binding(nameof(CloseButtonTemplate)) { Source = this });
            appPanel.SetResourceReference(UctVirtualDesktopApplicationPanel.HeaderButtonHighlightBackgroundProperty,
                "UctTaskbarHighlightedColor");
            appPanel.SetBinding(UctVirtualDesktopApplicationPanel.IconProperty,
                new Binding(nameof(RunningAppInfo.ModuleDefinition.Icon)) { Source = appInfo.ModuleDefinition });
            appPanel.SetBinding(UctVirtualDesktopApplicationPanel.ApplicationTitleProperty,
                new Binding(nameof(RunningAppInfo.ModuleDefinition.AppName)) { Source = appInfo.ModuleDefinition });
            appPanel.SetResourceReference(UctVirtualDesktopApplicationPanel.ForegroundProperty,
                "UctCommonForegroundColor");
            SetAppInfo(appPanel, appInfo);
            appPanel.CloseRequested += (sender, args) =>
                CloseApplication(GetAppInfo(sender as UctVirtualDesktopApplicationPanel));
            appPanel.MinimizeRequested += (sender, args) =>
                MinimizeApplication(GetAppInfo(sender as UctVirtualDesktopApplicationPanel));
            appPanel.MaximizeRequested += (sender, args) =>
                ToogleApplicationMaximize(GetAppInfo(sender as UctVirtualDesktopApplicationPanel));
            appPanel.Activated += (sender, args) =>
                ActivateApplication(GetAppInfo(sender as UctVirtualDesktopApplicationPanel));
            appPanel.GlobalMouseOperationStarted += (sender, args) =>
            {
                _brdGlobalMouseActions.IsHitTestVisible = true;
                _brdGlobalMouseActions.Cursor = args.DesiredCursor;
                args.MouseHost = _brdGlobalMouseActions;
                args.Handled = true;
            };
            appPanel.GlobalMouseOperationFinished += (sender, args) =>
            {
                _brdGlobalMouseActions.IsHitTestVisible = false;
                _brdGlobalMouseActions.Cursor = null;
            };
            _gridContent.Children.Add(appPanel);
            appInfo.Panel = appPanel;
        }

        ActivateApplication(appInfo);
    }

    private void ActivateApplication(RunningAppInfo? appInfo)
    {
        foreach (UIElement item in _gridContent.Children)
        {
            if (item is UctVirtualDesktopApplicationPanel panel)
            {
                if (panel.Content == appInfo.InstancedUI)
                {
                    if (item.Visibility != Visibility.Visible)
                        item.Visibility = Visibility.Visible;
                    Grid.SetZIndex(panel, 2);
                }
                else
                {
                    Grid.SetZIndex(panel, 1);
                }
            }
        }

        foreach (var item in _runningAppInfos)
        {
            item.AppButton.IsSelected = item == appInfo;
        }
    }

    private void MinimizeApplication(RunningAppInfo? appInfo)
    {
        if (appInfo?.Panel != null)
            appInfo.Panel.Visibility = Visibility.Collapsed;
    }

    private void ToogleApplicationMaximize(RunningAppInfo? appInfo)
    {
        if (appInfo.Panel != null)
            appInfo.Panel.IsMaximized = !appInfo.Panel.IsMaximized;
    }

    private void CloseApplication(RunningAppInfo? appInfo)
    {
        if (appInfo != null && _runningAppInfos.Contains(appInfo))
        {
            UIElement? removePanel = null;
            foreach (UIElement item in _gridContent.Children)
            {
                if (item is UctVirtualDesktopApplicationPanel panel && panel.Content == appInfo.InstancedUI)
                {
                    removePanel = item;
                    break;
                }
            }

            _runningAppInfos.Remove(appInfo);
            _applicationPanel.Children.Remove(appInfo.AppButton);
            if (removePanel != null)
                _gridContent.Children.Remove(removePanel);
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
        _brdModal.Visibility = _brdTaskbarMenu.Visibility = Visibility.Collapsed;
        if (e.ModuleDefinition.MaxInstances > 0 &&
            _runningAppInfos.Count(it => it.ModuleDefinition == e.ModuleDefinition) >= e.ModuleDefinition.MaxInstances)
        {
            var app = _runningAppInfos.FirstOrDefault(it => it.ModuleDefinition == e.ModuleDefinition);
            if (app != null)
                LaunchOrResumeApplication(app);
            return;
        }

        var appButton = new UctImageButton()
        {
            ContentTemplate = e.ModuleDefinition.Icon ?? DefaultAppIcon, ToolTip = e.ModuleDefinition.AppName
        };
        appButton.SetBinding(UctImageButton.HeightProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        appButton.SetBinding(UctImageButton.WidthProperty, new Binding(nameof(TaskbarSize)) { Source = this });
        appButton.SetResourceReference(UctImageButton.HighlightBackgroundProperty, "UctTaskbarHighlightedColor");
        appButton.SetResourceReference(UctImageButton.SelectedBackgroundProperty, "UctTaskbarSelectedColor");
        appButton.MouseLeftButtonDown += AppButton_OnMouseLeftButtonDown;
        RunningAppInfo newApp = new RunningAppInfo()
        {
            ModuleDefinition = e.ModuleDefinition,
            InstancedUI = e.ModuleDefinition.GetModuleUI(),
            AppButton = appButton
        };
        LaunchOrResumeApplication(newApp);
    }

    private void AppButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var app = _runningAppInfos.FirstOrDefault(it => it.AppButton == sender);
        if (app != null)
            LaunchOrResumeApplication(app);
    }

    private void MnuBackground_OnClick(object sender, RoutedEventArgs e)
    {
        if (CanUserChangeBackground)
        {
            var dia = new OpenFileDialog()
            {
                Filter = "Image Files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp"
            };
            if (dia.ShowDialog() == true)
            {
                DesktopBackgroundImage = dia.FileName;
            }
        }
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

    public class RunningAppInfo
    {
        public UctModuleDefinition ModuleDefinition { get; set; }

        public UIElement InstancedUI { get; set; }

        public UctImageButton AppButton { get; set; }

        public UctVirtualDesktopApplicationPanel Panel { get; set; }
    }
}

public enum TaskbarPlacement
{
    Bottom,
    Left,
    Top,
    Right
}