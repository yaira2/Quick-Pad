using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ZoomIn : SimpleCommand<DocumentViewModel>
    {
        public ZoomIn()
        {
            Executioner = viewModel =>
            {
                if (viewModel.ScaleValue <= 4)
                {
                    viewModel.ScaleValue = viewModel.ScaleValue + (10 / 100);
                }

                return Task.CompletedTask;
            };
        }
    }
}