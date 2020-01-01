using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands
{
    public class ToggleWordWrapCommand : SimpleCommand<DocumentViewModel>
    {
        public ToggleWordWrapCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.CurrentWordWrap = !viewModel.CurrentWordWrap;

                return Task.CompletedTask;
            };
        }
    }
}