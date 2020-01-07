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
                viewModel.Document.CurrentWordWrap = !viewModel.Document.CurrentWordWrap;

                return Task.CompletedTask;
            };
        }
    }
}