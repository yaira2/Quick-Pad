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
                }
                else
                {
                    documentViewModel.ShowMarkdownViewer = true; //show the markdown preview
                }
                return Task.CompletedTask;
            };
        }
    }
}