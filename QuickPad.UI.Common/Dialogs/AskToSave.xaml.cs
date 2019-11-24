using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class AskToSave
    {
        private DocumentViewModel _viewModel;
        public VisualThemeSelector VisualThemeSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }

        public DocumentViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public AskToSave(QuickPadCommands commands)
        {
            Commands = commands;
            this.InitializeComponent();
        }

        private void AskToSave_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }
    }
}
