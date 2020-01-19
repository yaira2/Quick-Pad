using System.Linq;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ReplaceAllCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ReplaceAllCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var results = findAndReplace.ReplaceAll(documentViewModel);
                if (results.Length > 0) documentViewModel.Text = results.Last().text;
                return Task.CompletedTask;
            };
        }
    }
}