using Windows.Storage;
using Windows.Storage.Streams;
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
        private DocumentViewModel<StorageFile, IRandomAccessStream> _viewModel;
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands { get; }
        public WindowsSettingsViewModel Settings { get; }

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public WelcomeDialog(QuickPadCommands<StorageFile, IRandomAccessStream> commands, WindowsSettingsViewModel settings)
        {
            Settings = settings;
            Commands = commands;
            this.InitializeComponent();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }
    }
}
