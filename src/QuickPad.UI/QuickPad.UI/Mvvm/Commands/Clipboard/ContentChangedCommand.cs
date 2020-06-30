using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Commands.Clipboard
{
    public class ContentChangedCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, IContentChangedCommand<StorageFile, IRandomAccessStream>
    {
        public ContentChangedCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = async viewModel =>
            {
                //send the selected text to the clipboard
                try
                {
                    var clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                    var dataPackage = new DataPackage();
                    if (clipboardContent.Contains(StandardDataFormats.Text))
                    {
                        dataPackage.SetText(await clipboardContent.GetTextAsync());
                        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                        Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                    }
                }
                catch
                {
                    // Don't let the clipboard take down the thread
                }
            };
        }
    }
}