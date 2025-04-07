using System.Windows;

namespace UniversalControlToolkit.WPF.Styling;

public class CombinedStyleEngine : DependencyObject
{
    private static readonly IDictionary<string, Style> _generatedStyles = new Dictionary<string, Style>();

    private static readonly IList<ResourceDictionary> _resources = new List<ResourceDictionary>();

    public static void RegisterDictionary(string source)
    {
        _resources.Add(new ResourceDictionary() { Source = new Uri(source) });
    }

    public static void ClearRegisteredDictionaries()
    {
        _resources.Clear();
    }

    public string? CombinedStyle
    {
        get => (string?)GetValue(CombinedStyleProperty);
        set => SetValue(CombinedStyleProperty, value);
    }

    public static readonly DependencyProperty CombinedStyleProperty =
        DependencyProperty.RegisterAttached(nameof(CombinedStyle), typeof(string), typeof(CombinedStyleEngine),
            new PropertyMetadata(null, CombinedStyleChanged));

    private static void CombinedStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FrameworkElement sender = d as FrameworkElement;
        string? newValue = e.NewValue as string;
        sender.SetValue(FrameworkElement.StyleProperty, GetStyleForName(newValue));
    }

    public static string? GetCombinedStyle(FrameworkElement element)
    {
        return element.GetValue(CombinedStyleProperty) as string;
    }

    public static void SetCombinedStyle(FrameworkElement element, string? value)
    {
        element.SetValue(CombinedStyleProperty, value);
    }

    private static Style? GetStyleForName(string? seStyle)
    {
        if (string.IsNullOrWhiteSpace(seStyle))
            return null;
        string[] names = seStyle.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).OrderBy(it => it).ToArray();
        string rejoined = string.Join(' ', names);
        if (!_generatedStyles.ContainsKey(rejoined))
        {
            _generatedStyles[rejoined] = BuildJointStyle(names);
        }

        return _generatedStyles[rejoined];
    }

    private static Style BuildJointStyle(string[] styles)
    {
        Style jointStyle = new Style();
        Dictionary<DependencyProperty, Tuple<SetterBase, int>> _settersByImportanceLevel =
            new Dictionary<DependencyProperty, Tuple<SetterBase, int>>();
        foreach (var style in styles)
        {
            foreach (var resDict in _resources)
            {
                if (resDict.Contains(style) && resDict[style] is Style resStyle)
                {
                    foreach (var setterBase in resStyle.Setters)
                    {
                        if (setterBase is Setter setter)
                        {
                            int importance = (setter as CombinedStyleSetter)?.ImportanceLevel ?? 0;
                            if (!_settersByImportanceLevel.ContainsKey(setter.Property) ||
                                _settersByImportanceLevel[setter.Property].Item2 < importance)
                            {
                                _settersByImportanceLevel[setter.Property] =
                                    new Tuple<SetterBase, int>(setter, importance);
                            }
                        }
                    }

                    break;
                }
            }
        }

        foreach (var setter in _settersByImportanceLevel.Values)
        {
            jointStyle.Setters.Add(setter.Item1);
        }

        return jointStyle;
    }
}