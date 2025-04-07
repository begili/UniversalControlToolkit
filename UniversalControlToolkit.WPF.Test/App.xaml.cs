using System.Configuration;
using System.Data;
using System.Windows;
using UniversalControlToolkit.WPF.Styling;
using UniversalControlToolkit.WPF.Utils;

namespace UniversalControlToolkit.WPF.Test;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public bool IsDarkModeActive { get; private set; }

    public App()
    {
        ThemeController.RegisterTheme("light", false, null);
        ThemeController.RegisterTheme("dark", true, null);
        CombinedStyleEngine.RegisterDictionary(
            "pack://application:,,,/UniversalControlToolkit.WPF.Test;component/StylingTest/SeparatedStyles.xaml");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        SetTheme(true);
        base.OnStartup(e);
    }

    public void SetTheme(bool darkMode)
    {
        ThemeController.SetTheme(darkMode ? "dark" : "light");
        IsDarkModeActive = darkMode;
    }
}