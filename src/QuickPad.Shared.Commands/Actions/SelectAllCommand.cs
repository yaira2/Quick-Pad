using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class SelectAllCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public SelectAllCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.SelectText(0, documentViewModel.Text.Length, false);
                return Task.CompletedTask;
            };
        }
    }
}