using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class WelcomeDialog
    {
        public IVisualThemeSelector VTSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }

        public WelcomeDialog()
        {
            this.InitializeComponent();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
