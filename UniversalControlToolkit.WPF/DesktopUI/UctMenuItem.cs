using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using UniversalControlToolkit.WPF.DesktopUI.Utils;

namespace UniversalControlToolkit.WPF.DesktopUI;

[ContentProperty(nameof(Children))]
public class UctMenuItem : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    public event EventHandler<ModuleDefinitionClickedEventArgs>? ModuleDefinitionClicked;

    private readonly Grid _grdHost;
    private readonly UctMenu _childMenu;
    private readonly CtkMenuItemCollection _children;
    private readonly Border _brdHightlight;
    private readonly ContentPresenter _cpIcon;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctMenuItem()
    {
        RowDefinition rdFirstRow, rdSecondRow;
        ColumnDefinition cdFirstCol;
        _grdHost = new Grid()
        {
            RowDefinitions =
            {
                (rdFirstRow = new RowDefinition()),
                (rdSecondRow = new RowDefinition())
            },
            ColumnDefinitions =
            {
                (cdFirstCol = new ColumnDefinition()),
                new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }
            },
            IsHitTestVisible = true,
            Background = Brushes.Transparent
        };
        rdFirstRow.SetBinding(RowDefinition.HeightProperty, new Binding(nameof(RowHeight)) { Source = this });
        rdSecondRow.SetBinding(RowDefinition.HeightProperty,
            new Binding(nameof(IsExpanded)) { Source = this, Converter = new IsExpandedToRowHeightConverter() });
        cdFirstCol.SetBinding(ColumnDefinition.WidthProperty, new Binding(nameof(RowHeight)) { Source = this });

        _brdHightlight = new Border()
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = true
        };
        Grid.SetColumnSpan(_brdHightlight, 2);
        _brdHightlight.MouseEnter += BrdHightlight_OnMouseEnter;
        _brdHightlight.MouseLeave += BrdHightlight_OnMouseLeave;
        _grdHost.Children.Add(_brdHightlight);

        _cpIcon = new ContentPresenter();
        _cpIcon.SetBinding(ContentPresenter.ContentTemplateProperty,
            new Binding(nameof(AppIcon)) { Source = this });

        Viewbox vb = new Viewbox()
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = _cpIcon,
            Margin = new Thickness(2)
        };
        _grdHost.Children.Add(vb);


        _children = new CtkMenuItemCollection();
        _children.CollectionChanged += Children_OnCollectionChanged;
        var cpContent = new ContentPresenter()
        {
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        cpContent.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(Content)) { Source = this });
        Grid.SetColumn(cpContent, 1);
        _grdHost.Children.Add(cpContent);

        _childMenu = new UctMenu();
        _childMenu.SetBinding(UctMenu.RowHeightProperty, new Binding(nameof(RowHeight)) { Source = this });
        _childMenu.SetBinding(UctMenu.SubMenuInsetProperty, new Binding(nameof(SubMenuInset)) { Source = this });
        _childMenu.SetBinding(UctMenu.GroupIconProperty, new Binding(nameof(GroupIcon)) { Source = this });
        _childMenu.SetBinding(UctMenu.MarginProperty,
            new Binding(nameof(SubMenuInset)) { Source = this, Converter = new DoubleToLeftMarginConverter() });
        _childMenu.SetBinding(UctMenu.HighlightBackgroundProperty,
            new Binding(nameof(HighlightBackground)) { Source = this });
        Grid.SetColumnSpan(_childMenu, 2);
        Grid.SetRow(_childMenu, 1);
        _childMenu.ModuleDefinitionClicked += ChildMenu_OnModuleDefinitionClicked;
        _grdHost.Children.Add(_childMenu);
        AddVisualChild(_grdHost);
    }

    //--------------------------
    //
    //      properties
    //
    //--------------------------

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(UctMenuItem), new PropertyMetadata(false));

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(UctMenuItem), new PropertyMetadata(32.0));

    public double SubMenuInset
    {
        get => (double)GetValue(SubMenuInsetProperty);
        set => SetValue(SubMenuInsetProperty, value);
    }

    public static readonly DependencyProperty SubMenuInsetProperty =
        DependencyProperty.Register(nameof(SubMenuInset), typeof(double), typeof(UctMenuItem),
            new PropertyMetadata(16.0));

    protected override int VisualChildrenCount => 1;

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(UctMenuItem), new PropertyMetadata(null));

    public UctModuleDefinition? ModuleDefinition
    {
        get => (UctModuleDefinition?)GetValue(ModuleDefinitionProperty);
        set => SetValue(ModuleDefinitionProperty, value);
    }

    public static readonly DependencyProperty ModuleDefinitionProperty =
        DependencyProperty.Register(nameof(ModuleDefinition), typeof(UctModuleDefinition), typeof(UctMenuItem),
            new PropertyMetadata(null));

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public CtkMenuItemCollection Children => _children;

    public DataTemplate GroupIcon
    {
        get => (DataTemplate)GetValue(GroupIconProperty);
        set => SetValue(GroupIconProperty, value);
    }

    public static readonly DependencyProperty GroupIconProperty =
        DependencyProperty.Register(nameof(GroupIcon), typeof(DataTemplate), typeof(UctMenuItem),
            new PropertyMetadata(null));

    public DataTemplate AppIcon
    {
        get => (DataTemplate)GetValue(AppIconProperty);
        set => SetValue(AppIconProperty, value);
    }

    public static readonly DependencyProperty AppIconProperty =
        DependencyProperty.Register(nameof(AppIcon), typeof(DataTemplate), typeof(UctMenuItem),
            new PropertyMetadata(null));

    public Brush HighlightBackground
    {
        get => (Brush)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }

    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.Register(nameof(HighlightBackground), typeof(Brush), typeof(UctMenuItem),
            new PropertyMetadata(Brushes.SkyBlue));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _grdHost;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (Children.Count > 0)
            IsExpanded = !IsExpanded;
        else if (ModuleDefinition != null)
        {
            ModuleDefinitionClicked?.Invoke(this,
                new ModuleDefinitionClickedEventArgs(ModuleDefinition));
        }

        e.Handled = true;
    }

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    private void Children_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dictionary<int, UctMenuItem> addedItems;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                addedItems = new Dictionary<int, UctMenuItem>();
                if (e.NewItems != null)
                    foreach (UctMenuItem item in e.NewItems)
                        addedItems.Add(Children.IndexOf(item), item);
                foreach (var item in addedItems.Keys.OrderBy(it => it))
                    _childMenu.Items.Insert(item, addedItems[item]);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                    foreach (UctMenuItem item in e.OldItems)
                        _childMenu.Items.Remove(item);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                    foreach (UctMenuItem item in e.OldItems)
                        _childMenu.Items.Remove(item);
                addedItems = new Dictionary<int, UctMenuItem>();
                if (e.NewItems != null)
                    foreach (UctMenuItem item in e.NewItems)
                        addedItems.Add(Children.IndexOf(item), item);
                foreach (var item in addedItems.Keys.OrderBy(it => it))
                    _childMenu.Items.Insert(item, addedItems[item]);
                break;
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                _childMenu.Items.Clear();
                for (int i = 0; i < _children.Count; i++)
                {
                    var item = _children[i];
                    _childMenu.Items.Add(item);
                }

                break;
        }

        BindingOperations.ClearBinding(_cpIcon, ContentPresenter.ContentTemplateProperty);
        if (_children.Count > 0)
        {
            _cpIcon.SetBinding(ContentPresenter.ContentTemplateProperty,
                new Binding(nameof(GroupIcon)) { Source = this });
        }
        else
        {
            _cpIcon.SetBinding(ContentPresenter.ContentTemplateProperty,
                new Binding(nameof(AppIcon)) { Source = this });
        }
    }

    //--------------------------
    //
    //      events
    //
    //--------------------------

    private void BrdHightlight_OnMouseEnter(object sender, MouseEventArgs e)
    {
        _brdHightlight.Background = HighlightBackground;
    }

    private void BrdHightlight_OnMouseLeave(object sender, MouseEventArgs e)
    {
        _brdHightlight.Background = Brushes.Transparent;
    }

    private void ChildMenu_OnModuleDefinitionClicked(object? sender, ModuleDefinitionClickedEventArgs e)
    {
        ModuleDefinitionClicked?.Invoke(sender, e);
    }

    //--------------------------
    //
    //      classes
    //
    //--------------------------

    private class DoubleToLeftMarginConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double dVal)
                return new Thickness(dVal, 0, 0, 0);
            return new Thickness(0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private class IsExpandedToRowHeightConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool bVal && bVal)
                return GridLength.Auto;
            return new GridLength(0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

public class ModuleDefinitionClickedEventArgs : EventArgs
{
    public ModuleDefinitionClickedEventArgs(UctModuleDefinition moduleDefinition)
    {
        ModuleDefinition = moduleDefinition;
    }

    public UctModuleDefinition ModuleDefinition { get; set; }
}