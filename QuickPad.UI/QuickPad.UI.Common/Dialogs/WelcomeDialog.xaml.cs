using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class WelcomeDialog
    {
        private SettingsViewModel _settings;
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }
        public SettingsViewModel Settings => _settings;

        public WelcomeDialog(SettingsViewModel settings)
        {
            _settings = settings;
            this.InitializeComponent();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
