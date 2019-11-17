using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace QuickPad.MVVM
{
    public class PasteCommand : SimpleCommand<DocumentViewModel>
    {
        private SettingsViewModel _settings;
        private bool canPasteText;

        public PasteCommand(SettingsViewModel settingsViewModel) : base()
        {
            _settings = settingsViewModel;
        }

        public PasteCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.Document.Selection.Text.Length > 0;

            Executioner = async viewModel =>
            {
                if (_settings.PasteTextOnly)
                {
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                    {
                        //if there is nothing to paste then dont paste anything since it will crash
                        if (!string.IsNullOrEmpty(await dataPackageView.GetTextAsync()))
                        {
                            viewModel.Document.Selection.TypeText(await dataPackageView.GetTextAsync()); //paste the text from the clipboard
                        }
                    }
                }
                else
                {
                    viewModel.Document.Selection.Paste(0);
                }
            };
        }

        private bool CanPasteText
        {
            get => canPasteText; 
            set
            { 
                canPasteText = value;

                InvokeCanExecuteChanged(this);
            }
        }

       
        private async void ClipboardStatusUpdate(object sender, object e)
        {
            try
            {
                DataPackageView clipboardContent = Clipboard.GetContent();
                Clipboard.ContentChanged -= ClipboardStatusUpdate;
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
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
                Clipboard.ContentChanged += ClipboardStatusUpdate;
                CanPasteText = clipboardContent.Contains(StandardDataFormats.Text);
            }
            catch (Exception)
            {
                CanPasteText = false;
            }
        }

    }

}
