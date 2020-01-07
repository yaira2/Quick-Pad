using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class StrikeThroughCommand : SimpleCommand<DocumentViewModel>
    {
        public StrikeThroughCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();
                var selectedText = viewModel.SelectedText;
                if (string.IsNullOrWhiteSpace(selectedText))
                {
                    viewModel.Document.SelStrikethrough = !viewModel.Document.SelStrikethrough;
                }

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}