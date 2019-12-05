using Windows.UI;
using Windows.UI.Xaml;

namespace QuickPad.Mvvm.Models.Theme
{
    public class DefaultTextForegroundColor
    {
        private IVisualThemeSelector _visualThemeSelector;

        public DefaultTextForegroundColor(IVisualThemeSelector visualThemeSelector)
        {
            _visualThemeSelector = visualThemeSelector;
        }

        public Color Color => _visualThemeSelector.CurrentItem.DefaultTextForegroundColor;
    }
}
