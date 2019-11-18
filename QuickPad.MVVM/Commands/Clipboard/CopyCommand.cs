using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVVM.Commands.Clipboard
{
    public class CopyCommand : SimpleCommand<DocumentViewModel>
    {
        public CopyCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.Document.Selection.Text.Length > 0;

            Executioner = viewModel =>
            {
                //send the selected text to the clipboard
                var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
                dataPackage.SetText(viewModel.Document.Selection.Text);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();

                return Task.CompletedTask;
            };
        }
    }
}