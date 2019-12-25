using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;
using System;

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