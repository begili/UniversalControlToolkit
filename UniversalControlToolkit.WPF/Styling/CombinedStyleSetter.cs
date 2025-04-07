using System.Windows;

namespace UniversalControlToolkit.WPF.Styling;

public class CombinedStyleSetter : Setter
{
    public int ImportanceLevel { get; set; } = 0;
}