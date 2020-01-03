using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class SuperscriptCommand : SimpleCommand<DocumentViewModel>
    {
        public SuperscriptCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();

                viewModel.Document.Selection.CharacterFormat.Superscript = FormatEffect.Toggle;

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}