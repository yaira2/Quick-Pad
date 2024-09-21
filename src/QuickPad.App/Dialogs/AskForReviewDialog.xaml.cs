using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.App.Helpers;
using QuickPad.App.Theme;
using System;
using Windows.Storage;
using Windows.Storage.Streams;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.App.Dialogs
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

        private void CmdClose_Click(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        public new event Action Closed;
    }
}