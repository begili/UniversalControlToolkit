using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI;

[ContentProperty(nameof(Items))]
public class UctMenu : Control
{
    //--------------------------
    //
    //      fields
    //
    //--------------------------

    public event EventHandler<ModuleDefinitionClickedEventArgs>? ModuleDefinitionClicked;

    private readonly StackPanel _itemHost;
    private readonly CtkMenuItemCollection _items;

    //--------------------------
    //
    //      constructor
    //
    //--------------------------

    public UctMenu()
    {
        _items = new CtkMenuItemCollection();
        _items.CollectionChanged += Items_OnCollectionChanged;
        _itemHost = new StackPanel();
        AddVisualChild(_itemHost);
    }

    //--------------------------
    //
    //      property
    //
    //--------------------------

    protected override int VisualChildrenCount => 1;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public CtkMenuItemCollection Items => _items;

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(UctMenu), new PropertyMetadata(32.0));

    public double SubMenuInset
    {
        get => (double)GetValue(SubMenuInsetProperty);
        set => SetValue(SubMenuInsetProperty, value);
    }

    public static readonly DependencyProperty SubMenuInsetProperty =
        DependencyProperty.Register(nameof(SubMenuInset), typeof(double), typeof(UctMenu), new PropertyMetadata(16.0));

    //--------------------------
    //
    //      methods
    //
    //--------------------------

    protected override Visual GetVisualChild(int index) => _itemHost;

    private void ItemModuleDefinitionClicked(object? sender, ModuleDefinitionClickedEventArgs e)
    {
        ModuleDefinitionClicked?.Invoke(sender, e);
    }

    //--------------------------
    //
    //      events
    //
    //--------------------------

    private void Items_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dictionary<int, UctMenuItem> addedItems;
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                addedItems = new Dictionary<int, UctMenuItem>();
                if (e.NewItems != null)
                    foreach (UctMenuItem item in e.NewItems)
                        addedItems.Add(Items.IndexOf(item), item);
                foreach (var item in addedItems.Keys.OrderBy(it => it))
                {
                    _itemHost.Children.Insert(item, addedItems[item]);
                    addedItems[item].ModuleDefinitionClicked += ItemModuleDefinitionClicked;
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                    foreach (UctMenuItem item in e.OldItems)
                    {
                        _itemHost.Children.Remove(item);
                        item.ModuleDefinitionClicked -= ItemModuleDefinitionClicked;
                    }

                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                    foreach (UctMenuItem item in e.OldItems)
                    {
                        _itemHost.Children.Remove(item);
                        item.ModuleDefinitionClicked -= ItemModuleDefinitionClicked;
                    }

                addedItems = new Dictionary<int, UctMenuItem>();
                if (e.NewItems != null)
                    foreach (UctMenuItem item in e.NewItems)
                        addedItems.Add(Items.IndexOf(item), item);
                foreach (var item in addedItems.Keys.OrderBy(it => it))
                {
                    _itemHost.Children.Insert(item, addedItems[item]);
                    addedItems[item].ModuleDefinitionClicked += ItemModuleDefinitionClicked;
                }

                break;
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                foreach (UIElement item in _itemHost.Children)
                    if (item is UctMenuItem cmi)
                        cmi.ModuleDefinitionClicked -= ItemModuleDefinitionClicked;
                _itemHost.Children.Clear();
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    item.SetBinding(UctMenuItem.RowHeightProperty, new Binding(nameof(RowHeight)) { Source = this });
                    _itemHost.Children.Add(item);
                    item.ModuleDefinitionClicked += ItemModuleDefinitionClicked;
                }

                break;
        }
    }
}

public class CtkMenuItemCollection : ObservableCollection<UctMenuItem>;