using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniversalControlToolkit.WPF.DesktopUI.Utils;
using UniversalControlToolkit.WPF.Test.Views;

namespace UniversalControlToolkit.WPF.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void App1_OnModuleCreateRequest(object? sender, UctModuleCreateEventArgs e)
    {
        e.ModuleUI = new TextBlock()
        {
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 40, Text = "APP 1"
        };
        e.Handled = true;
    }

    private void App2_OnModuleCreateRequest(object? sender, UctModuleCreateEventArgs e)
    {
        e.ModuleUI = new TextBlock()
        {
            VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 40, Text = "APP 2"
        };
        e.Handled = true;
    }

    private void BtnSelectTheme_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        (App.Current as App).SetTheme(!(App.Current as App).IsDarkModeActive);
    }

    private void BtnShutdown_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        App.Current.Shutdown(0);
    }

    private void UctModuleDefinition_OnModuleCreateRequest(object? sender, UctModuleCreateEventArgs e)
    {
        e.ModuleUI = new StyleTestView();
        e.Handled = true;
    }
}