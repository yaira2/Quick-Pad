using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class RedoCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public RedoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.CanRedo;

            Executioner = viewModel =>
            {
                viewModel.RequestRedo();

                return Task.CompletedTask;
            };
        }
    }
}