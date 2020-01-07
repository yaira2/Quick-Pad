using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class JustifyCommand : SimpleCommand<DocumentViewModel>
    {
        public JustifyCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.SelJustify = true;
                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}