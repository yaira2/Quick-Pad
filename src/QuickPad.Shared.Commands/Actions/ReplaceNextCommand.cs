using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ReplaceNextCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ReplaceNextCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var (text, _, start, _) = findAndReplace.ReplaceNext(documentViewModel);
                if (start > -1) documentViewModel.SetText(text, true);
                return Task.CompletedTask;
            };
        }
    }
}