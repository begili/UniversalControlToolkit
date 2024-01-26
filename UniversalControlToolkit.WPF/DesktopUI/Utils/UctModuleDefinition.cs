using System.Windows;
using System.Windows.Media;

namespace UniversalControlToolkit.WPF.DesktopUI.Utils;

public class UctModuleDefinition
{
    public event EventHandler<UcsModuleCreateEventArgs>? ModuleCreateRequest;

    public ImageSource Icon { get; set; }

    public int MaxInstances { get; set; } = 0;

    public UIElement? GetModuleUI()
    {
        UcsModuleCreateEventArgs args = new UcsModuleCreateEventArgs();
        ModuleCreateRequest?.Invoke(this, args);
        return args.ModuleUI;
    }
}

public class UcsModuleCreateEventArgs : EventArgs
{
    public UIElement ModuleUI { get; set; }

    public bool Handled { get; set; }
}