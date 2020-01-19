using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ResetZoomCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ResetZoomCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.ScaleValue = 1;

                return Task.CompletedTask;
            };
        }
    }
}