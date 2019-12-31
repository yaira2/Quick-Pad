using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ZoomInCommand : SimpleCommand<DocumentViewModel>
    {
        public ZoomInCommand()
        {
            Executioner = viewModel =>
            {
                if (viewModel.ScaleValue <= 4)
                {
                    viewModel.ScaleValue += 0.1f;
                }

                return Task.CompletedTask;
            };
        }
    }
}