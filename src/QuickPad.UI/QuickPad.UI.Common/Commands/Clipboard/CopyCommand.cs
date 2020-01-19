using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Clipboard
{

    public class CopyCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, ICopyCommand<StorageFile, IRandomAccessStream>
    {
        public CopyCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = viewModel =>
            {
                //send the selected text to the clipboard
                var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
                dataPackage.SetText(viewModel.SelectedText);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();

                return Task.CompletedTask;
            };
        }
    }
}