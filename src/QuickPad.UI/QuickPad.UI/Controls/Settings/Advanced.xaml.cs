using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class Advanced : Page
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        
        public SettingsViewModel Settings { get; } = App.Settings;

        public QuickPadCommands Commands => App.Commands;

        public Advanced()
        {
            this.InitializeComponent();
            DataContext = Settings;
        }
    }
}
