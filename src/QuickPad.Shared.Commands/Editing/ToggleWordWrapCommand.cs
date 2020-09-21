using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands
{
    public class ToggleWordWrapCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ToggleWordWrapCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.CurrentWordWrap = !viewModel.Document.CurrentWordWrap;

                return Task.CompletedTask;
            };
        }
    }
}