using System.Windows;
using System.Windows.Input;

namespace UniversalControlToolkit.WPF.DesktopUI.Utils;

public class GlobalMouseOperationEventArgs : EventArgs
{
    public IInputElement MouseHost { get; set; }
    
    public Cursor DesiredCursor { get; set; }
    
    public bool Handled { get; set; }
}