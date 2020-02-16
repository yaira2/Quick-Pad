using System;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Theme;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Dialogs
{
    public sealed partial class AskToSave : IDialogView
    {
        private DocumentViewModel<StorageFile, IRandomAccessStream> _viewModel;
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands { get; }
        public ResourceLoader ResourceLoader { get; }

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public AskToSave(QuickPadCommands<StorageFile, IRandomAccessStream> commands, ResourceLoader resourceLoader)
        {
            Commands = commands;
            ResourceLoader = resourceLoader;
            this.InitializeComponent();

            this.Title = ResourceLoader.GetString("SaveDialog/Title");
            this.PrimaryButtonText = ResourceLoader.GetString("SaveDialog/PrimaryButtonText");
            this.SecondaryButtonText = ResourceLoader.GetString("SaveDialog/SecondaryButtonText");
            this.CloseButtonText = ResourceLoader.GetString("SaveDialog/CloseButtonText");
            Content.Text = ResourceLoader.GetString("WouldYouLikeToSave/Text");

            base.Closed += (sender, args) => this.Closed?.Invoke();
        }

        private void AskToSave_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

        public new event Action Closed;
    }
}
