using System.Windows;

namespace UniversalControlToolkit.WPF.Utils;

public static class ThemeController
{
    private static readonly Dictionary<string, Tuple<bool, IEnumerable<Uri>?>> _sThemeResources =
        new Dictionary<string, Tuple<bool, IEnumerable<Uri>?>>();

    private static IEnumerable<ResourceDictionary>? _sLoadedResources;

    public static void AddGlobalResourceDictionary(Uri uri)
    {
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
    }

    public static void SetTheme(string themeName)
    {
        if (!_sThemeResources.ContainsKey(themeName))
            throw new ArgumentException("Invalid theme name");

        if (_sLoadedResources != null)
        {
            foreach (var resourceDictionary in _sLoadedResources)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
            }

            _sLoadedResources = null;
        }

        var contents = _sThemeResources[themeName];
        var uri = new Uri(
            $"pack://application:,,,/UniversalControlToolkit.WPF;component/Resources/{(contents.Item1 ? "DarkTheme.xaml" : "LightTheme.xaml")}",
            UriKind.Absolute);
        List<ResourceDictionary> loadedResources = new List<ResourceDictionary>();

        ResourceDictionary defResDict = new ResourceDictionary() { Source = uri };
        loadedResources.Add(defResDict);
        Application.Current.Resources.MergedDictionaries.Add(defResDict);

        if (contents.Item2 != null)
        {
            foreach (var resourceDictUri in contents.Item2)
            {
                ResourceDictionary resDict = new ResourceDictionary() { Source = resourceDictUri };
                loadedResources.Add(resDict);
                Application.Current.Resources.MergedDictionaries.Add(defResDict);
            }
        }

        _sLoadedResources = loadedResources;
    }

    public static void RegisterTheme(string themeName, bool isDarkMode, IEnumerable<Uri>? resources)
    {
        _sThemeResources.Add(themeName, new Tuple<bool, IEnumerable<Uri>?>(isDarkMode, resources));
    }
}