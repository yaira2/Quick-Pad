using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Clipboard
{
    public class DeleteCommand : SimpleCommand<DocumentViewModel>
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