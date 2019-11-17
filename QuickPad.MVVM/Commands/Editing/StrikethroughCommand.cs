using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class StrikethroughCommand : SimpleCommand<DocumentViewModel>
    {
        public StrikethroughCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();
                ITextSelection selectedText = viewModel.Document.Selection;
                if (selectedText != null)
                {
                    ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                    charFormatting.Strikethrough = FormatEffect.Toggle;
                    selectedText.CharacterFormat = charFormatting;
                }
                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }

}
