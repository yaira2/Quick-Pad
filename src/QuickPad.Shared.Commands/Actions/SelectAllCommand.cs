using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

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