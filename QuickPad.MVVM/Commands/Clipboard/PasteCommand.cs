using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVVM.Commands.Clipboard
{
    public class PasteCommand : SimpleCommand<DocumentViewModel>
    {
        private readonly SettingsViewModel _settings;
        private bool _canPasteText;

        public PasteCommand(SettingsViewModel settingsViewModel)
        {
            _settings = settingsViewModel;
        }

        public PasteCommand()
        {
            Task.Run(CheckClipboardStatus);

            CanExecuteEvaluator = viewModel => CanPasteText;

            Executioner = async viewModel =>
            {
                if (_settings.PasteTextOnly)
                {
                    var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                        //if there is nothing to paste then don't paste anything since it will crash
                        if (!string.IsNullOrEmpty(await dataPackageView.GetTextAsync()))
                            viewModel.Document.Selection.TypeText(
                                await dataPackageView.GetTextAsync()); //paste the text from the clipboard
                }
                else
                {
                    viewModel.Document.Selection.Paste(0);
                }

                viewModel.OnPropertyChanged(nameof(viewModel.Text));
            };
        }

        private bool CanPasteText
        {
            get => _canPasteText;
            set
            {
                _canPasteText = value;

                InvokeCanExecuteChanged(this);
            }
        }

        private async void ClipboardStatusUpdate(object sender, object e) => await CheckClipboardStatus();

        private async Task CheckClipboardStatus()
        {
            try
            {
                var clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged -= ClipboardStatusUpdate;
                var dataPackage = new DataPackage();
                if (_settings.PasteTextOnly)
                {
                    dataPackage.SetText(await clipboardContent.GetTextAsync());
                }
                else
                {
                    dataPackage = new DataPackage();
                    if (clipboardContent.Contains(StandardDataFormats.Rtf))
                        dataPackage.SetRtf(await clipboardContent.GetRtfAsync());
                    else
                        dataPackage.SetText(await clipboardContent.GetTextAsync());
                }

                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged += ClipboardStatusUpdate;
                CanPasteText = clipboardContent.Contains(StandardDataFormats.Text);
            }
            catch (Exception)
            {
                CanPasteText = false;
            }
        }
    }
}