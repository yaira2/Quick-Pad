using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class CenterAlignCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public CenterAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.SelCenter = true;

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}