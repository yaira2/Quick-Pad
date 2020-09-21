using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

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