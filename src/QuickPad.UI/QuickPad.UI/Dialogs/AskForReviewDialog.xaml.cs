using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Helpers;
using QuickPad.UI.Theme;
using System;
using Windows.Storage;
using Windows.Storage.Streams;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Dialogs
{
    public sealed partial class AskForReviewDialog : IDialogView
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

        public AskForReviewDialog(QuickPadCommands<StorageFile, IRandomAccessStream> commands
            , WindowsSettingsViewModel settings)
        {
            Settings = settings;
            Commands = commands;
            this.InitializeComponent();

            base.Closed += (sender, args) => this.Closed?.Invoke();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }

        public new event Action Closed;
    }
}