using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowMarkdownViewerCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ShowMarkdownViewerCommand()
        {
            Executioner = documentViewModel =>
            {
                if (documentViewModel.ShowMarkdownViewer) //the markdown preview is already showing
                {
                    documentViewModel.ShowMarkdownViewer = false; //hide the markdown preview
                    documentViewModel.TextBoxColumnSpan = 2; //reset the column span of the text box
                }
                else
                {
                    documentViewModel.ShowMarkdownViewer = true; //show the markdown preview
                    documentViewModel.TextBoxColumnSpan = 1; //set the column span of the text box
                }
                return Task.CompletedTask;
            };
        }
    }
}