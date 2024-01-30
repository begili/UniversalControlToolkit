using System.Windows;

namespace UniversalControlToolkit.WPF.Utils;

public static class ThemeController
{
    public static void SetTheme(bool isDarkMode)
    {
        var uri = new Uri($"pack://application:,,,/UniversalControlToolkit.WPF;component/Resources/{(isDarkMode ? "DarkTheme.xaml" : "LightTheme.xaml")}", UriKind.Absolute);
        SetTheme(uri);
    }
    
    public static void SetTheme(Uri resourceDictUri)
    {
        ResourceDictionary resDict = new ResourceDictionary() { Source = resourceDictUri };

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(resDict);
    }
}