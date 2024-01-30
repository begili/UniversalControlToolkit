using System.Windows;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI.Utils;

public class UctModuleDefinition
{
    public event EventHandler<UctModuleCreateEventArgs>? ModuleCreateRequest;

    public DataTemplate? Icon { get; set; }
    
    public string AppName { get; set; }

    public int MaxInstances { get; set; } = 0;

    public double DesiredHeight { get; set; } = double.NaN;

    public double DesiredWidth { get; set; } = double.NaN;

    public Thickness DesiredMargin { get; set; } = new Thickness(0);
    
    public bool IsMaximized { get; set; }

    public UIElement? GetModuleUI()
    {
        UctModuleCreateEventArgs args = new UctModuleCreateEventArgs();
        ModuleCreateRequest?.Invoke(this, args);
        return args.ModuleUI;
    }
}

public class UctModuleCreateEventArgs : EventArgs
{
    public UIElement ModuleUI { get; set; }

    public bool Handled { get; set; }
}