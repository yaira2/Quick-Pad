using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowFindCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ShowFindCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowReplace = false;
                documentViewModel.ShowFind = true;
                return Task.CompletedTask;
            };
        }
    }
}