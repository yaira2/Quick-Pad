using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class RightAlignCommand : SimpleCommand<DocumentViewModel>
    {
        public RightAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.SelRight = true;
                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}