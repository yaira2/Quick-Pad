using Windows.ApplicationModel.DataTransfer;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Clipboard
{
    public class ContentChangedCommand : SimpleCommand<DocumentViewModel>
    {
        public ContentChangedCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = async viewModel =>
            {
                //send the selected text to the clipboard
                DataPackageView clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                var dataPackage = new DataPackage();
                dataPackage.SetText(await clipboardContent.GetTextAsync());
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
            };
        }
    }
}