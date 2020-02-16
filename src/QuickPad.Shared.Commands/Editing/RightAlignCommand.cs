using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class RightAlignCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public RightAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.SelRight = true;
                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}