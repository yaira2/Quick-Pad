using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Commands.Clipboard
{
    public class DeleteCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, IDeleteCommand<StorageFile, IRandomAccessStream>
    {
        public DeleteCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = viewModel =>
            {
                //send the selected text to the clipboard
                viewModel.SelectedText = string.Empty;

                return Task.CompletedTask;
            };
        }
    }
}