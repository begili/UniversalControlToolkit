using System.Configuration;
using System.Data;
using System.Windows;
using UniversalControlToolkit.WPF.Utils;

namespace UniversalControlToolkit.WPF.Test;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeController.SetTheme(true);
        base.OnStartup(e);
    }
}