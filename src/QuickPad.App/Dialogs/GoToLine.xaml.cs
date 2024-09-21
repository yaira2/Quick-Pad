using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.App.Theme;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.App.Dialogs
{
    public sealed partial class GoToLine : IGoToLineView<StorageFile, IRandomAccessStream>
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands { get; }

        private DocumentViewModel<StorageFile, IRandomAccessStream> _viewModel;

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public async Task<bool> ShowAsyncByTask()
        {
            return (await ShowAsync(ContentDialogPlacement.Popup).AsTask<ContentDialogResult>()) == ContentDialogResult.Primary;
        }

        public GoToLine(QuickPadCommands<StorageFile, IRandomAccessStream> commands)
        {
            Commands = commands;
            this.InitializeComponent();

            base.Closed += (sender, args) => this.Closed?.Invoke();
        }

        private void CmdClose_Click(Windows.UI.Xaml.Controls.ContentDialog sender, Windows.UI.Xaml.Controls.ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void GotoTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //GotoTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            GotoTextBox.SelectAll();
        }

        private void GotoTextBox_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (GotoTextBox.Text.Length == 0) return;

            if (System.Int32.TryParse(GotoTextBox.Text, out var line) &&
                line > 0 &&
                line <= ViewModel.Document.LineIndices.Count)
            {
                ViewModel.LineToGoTo = line;
            }
            else
            {
                var old = GotoTextBox.Text.Substring(0, GotoTextBox.Text.Length - 1);
                GotoTextBox.Text = ViewModel.LineToGoTo.ToString();

                if (!old.StartsWith(GotoTextBox.Text))
                {
                    GotoTextBox.SelectAll();
                }
                else
                {
                    GotoTextBox.Select(old.Length, 0);
                }
            }
        }

        public new event Action Closed;
    }
}