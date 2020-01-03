using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.UI.Common.Theme;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class GoToLine : ContentDialog
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }

        private DocumentViewModel _viewModel;
        public DocumentViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public GoToLine()
        {
            this.InitializeComponent();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
