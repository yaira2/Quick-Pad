using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ReplaceNextCommand : SimpleCommand<DocumentViewModel>
    {
        public ReplaceNextCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var (text, _, start, _) = findAndReplace.ReplaceNext(documentViewModel);
                if (start > -1) documentViewModel.Text = text;
                return Task.CompletedTask;
            };
        }
    }
}