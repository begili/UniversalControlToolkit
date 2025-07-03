using System.Windows;
using System.Windows.Controls;
using UniversalControlToolkit.WPF.DesktopUI;

namespace UniversalControlToolkit.WPF.Test.SubAppWindows;

public partial class App2Window : UctVirtualDesktopWindow
{
    public App2Window()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        (new App1Window()).Show(this, true);
    }
}