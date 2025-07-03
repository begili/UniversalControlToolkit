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
using UniversalControlToolkit.WPF.Test.SubAppWindows;
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

    private void BtnSelectTheme_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        (App.Current as App).SetTheme(!(App.Current as App).IsDarkModeActive);
    }

    private void BtnShutdown_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        App.Current.Shutdown(0);
    }

    private void App1_OnClicked(object? sender, EventArgs e)
    {
        (new App1Window()).Show();
    }

    private void App2_OnClicked(object? sender, EventArgs e)
    {
        (new App2Window()).Show();
    }

    private void StyleTest_OnClicked(object? sender, EventArgs e)
    {
        (new StyleTestView()).Show();
    }
}