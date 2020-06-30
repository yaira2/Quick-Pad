using Windows.UI;
using Windows.UI.Xaml;

namespace QuickPad.UI.Theme
{
    public class DefaultTextForegroundColor
    {
        private IVisualThemeSelector _visualThemeSelector;

        public DefaultTextForegroundColor(IVisualThemeSelector visualThemeSelector)
        {
            _visualThemeSelector = visualThemeSelector;
        }

        public Color Color {
            get => _visualThemeSelector.CurrentItem.DefaultTextForegroundColor;
            set
            {
                _visualThemeSelector.CurrentItem.DefaultTextForegroundColor = value;
                if(!Application.Current.Resources.ContainsKey("DefaultTextForegroundColor"))
                {
                    Application.Current.Resources.Add("DefaultTextForegroundColor", value);
                }
                else
                {
                    Application.Current.Resources["DefaultTextForegroundColor"] = value;
                }
            }
        }
    }
}
