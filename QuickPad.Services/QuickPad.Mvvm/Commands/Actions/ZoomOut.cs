using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ZoomOut : SimpleCommand<DocumentViewModel>
    {
        public ZoomOut()
        {
            Executioner = viewModel =>
            {
                if (viewModel.ScaleValue >= 0.5)
                {
                    viewModel.ScaleValue = viewModel.ScaleValue - (10 / 100);
                }

                return Task.CompletedTask;
            };
        }
    }
}