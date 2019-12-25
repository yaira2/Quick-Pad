using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ZoomOutCommand : SimpleCommand<DocumentViewModel>
    {
        public ZoomOutCommand()
        {
            Executioner = viewModel =>
            {
                if (viewModel.ScaleValue >= 0.5)
                {
                    viewModel.ScaleValue -= 0.1f;
                }

                return Task.CompletedTask;
            };
        }
    }
}