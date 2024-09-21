using QuickPad.App.Helpers;
using QuickPad.App.Theme;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.App.Controls.Settings
{
    public sealed partial class Theme : Page
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public WindowsSettingsViewModel Settings => App.Settings;

        public Theme()
        {
            this.InitializeComponent();
        }
    }
}