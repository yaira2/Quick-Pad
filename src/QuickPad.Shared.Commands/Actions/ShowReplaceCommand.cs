using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowReplaceCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ShowReplaceCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowReplace = true;
                return Task.CompletedTask;
            };
        }
    }
}