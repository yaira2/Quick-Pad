using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class UndoCommand : SimpleCommand<DocumentViewModel>
    {
        public UndoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.Document.CanUndo();

            Executioner = viewModel =>
            {
                viewModel.Document.Undo(); //undo changes the user did to the text            

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}