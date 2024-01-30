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
    public bool IsDarkModeActive { get; private set; }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        SetTheme(true);
        base.OnStartup(e);
    }

    public void SetTheme(bool darkMode)
    {
        ThemeController.SetTheme(darkMode);
        IsDarkModeActive = darkMode;
    }
}