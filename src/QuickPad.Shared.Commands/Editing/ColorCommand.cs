using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ColorCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ColorCommand()
        {
            Executioner = viewModel =>
            {
                if (viewModel == null) return Task.CompletedTask;

                viewModel.Document.ForegroundColor = viewModel.FontColor;
                viewModel.OnPropertyChanged(nameof(viewModel.RtfText));

                return Task.CompletedTask;
            };
        }
    }
}