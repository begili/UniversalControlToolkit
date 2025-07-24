using System.Windows;
using System.Windows.Controls;
using UniversalControlToolkit.WPF.DesktopUI;

namespace UniversalControlToolkit.WPF.Test.SubAppWindows;

public partial class App1Window : UctVirtualDesktopWindow
{
    public App1Window()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        modalTest.Visibility = Visibility.Visible;
    }
}