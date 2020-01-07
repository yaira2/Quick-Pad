using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class SubscriptCommand : SimpleCommand<DocumentViewModel>
    {
        public SubscriptCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();

                viewModel.Document.SelSubscript = !viewModel.Document.SelSubscript;

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}