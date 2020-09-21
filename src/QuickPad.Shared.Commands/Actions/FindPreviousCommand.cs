using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class FindPreviousCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public FindPreviousCommand()
        {
            Executioner = documentViewModel =>
            {
                try
                {
                    var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                    findAndReplace.FindPrevious(documentViewModel);
                    return Task.CompletedTask;
                }
                finally
                {
                    documentViewModel.ReleaseUpdates();
                }
            };
        }
    }
}