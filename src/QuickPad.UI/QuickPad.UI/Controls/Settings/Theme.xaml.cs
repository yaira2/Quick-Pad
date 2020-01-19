using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class Theme : Page
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public SettingsViewModel Settings => App.Settings;

        public Theme()
        {
            this.InitializeComponent();
        }
    }
}
