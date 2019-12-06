using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class UpdateToolbar : SimpleCommand<DocumentViewModel>
    {
        private DocumentViewModel _viewModel;
        public UpdateToolbar()
        {
            Executioner = viewModel =>
            {
                var selectedText = viewModel.Document.Selection;
                if (selectedText != null)
                {
                    viewModel.SelBold =  Convert.ToBoolean(selectedText.CharacterFormat.Bold);
                    viewModel.SelItalic = Convert.ToBoolean(selectedText.CharacterFormat.Italic);
                    if (selectedText.CharacterFormat.Underline == UnderlineType.Single) { viewModel.SelUnderline = true; } else { viewModel.SelUnderline = false; }
                    viewModel.SelStrikethrough = Convert.ToBoolean(selectedText.CharacterFormat.Strikethrough);
                    viewModel.FontColor = selectedText.CharacterFormat.ForegroundColor;
                }

                return Task.CompletedTask;
            };
        }
    }
}