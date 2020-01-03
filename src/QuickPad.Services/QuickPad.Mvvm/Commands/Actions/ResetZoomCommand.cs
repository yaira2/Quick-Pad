using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ResetZoomCommand : SimpleCommand<DocumentViewModel>
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