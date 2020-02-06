using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class FindNextCommand : SimpleCommand<DocumentViewModel>
    {
        public FindNextCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                findAndReplace.FindNext(documentViewModel);
                return Task.CompletedTask;
            };
        }
    }
}