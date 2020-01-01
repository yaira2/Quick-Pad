using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class FindPreviousCommand : SimpleCommand<DocumentViewModel>
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