using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class RedoCommand : SimpleCommand<DocumentViewModel>
    {
        public RedoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.Document.CanRedo();

            Executioner = viewModel =>
            {
                viewModel.Document.Redo(); //undo changes the user did to the text            

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}