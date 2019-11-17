using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace QuickPad.MVVM
{
    public class CopyCommand : SimpleCommand<DocumentViewModel>
    {
        public CopyCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.Document.Selection.Text.Length > 0;

            Executioner = viewModel =>
            {
                //send the selected text to the clipboard
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(viewModel.Document.Selection.Text);
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();

                return Task.CompletedTask;
            };
        }
    }

}
