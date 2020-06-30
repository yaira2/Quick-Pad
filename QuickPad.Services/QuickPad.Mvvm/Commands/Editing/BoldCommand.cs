using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class BoldCommand : SimpleCommand<DocumentViewModel>
    {
        public BoldCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();
                //set the selected text to be bold if not already
                //if the text is already bold it will make it regular
                var selectedText = viewModel.Document.Selection;
                if (selectedText != null)
                {
                    var charFormatting = selectedText.CharacterFormat;
                    charFormatting.Bold = FormatEffect.Toggle;
                    selectedText.CharacterFormat = charFormatting;
                }

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}