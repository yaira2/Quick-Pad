using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class UndoCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public UndoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.CanUndo;

            Executioner = viewModel =>
            {
                viewModel.RequestUndo();

                return Task.CompletedTask;
            };
        }
    }
}