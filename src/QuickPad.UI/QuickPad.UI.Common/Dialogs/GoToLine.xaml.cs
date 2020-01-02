using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.UI.Common.Theme;
using QuickPad.Mvvm.Commands;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class GoToLine : ContentDialog
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }
        public GoToLine()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
