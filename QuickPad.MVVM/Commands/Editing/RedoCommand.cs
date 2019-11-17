using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;

namespace QuickPad.MVVM
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
