using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class HideFindCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public HideFindCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowFind = false;
                return Task.CompletedTask;
            };
        }
    }
}