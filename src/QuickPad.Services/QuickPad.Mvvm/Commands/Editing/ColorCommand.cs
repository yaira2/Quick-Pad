using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ColorCommand : SimpleCommand<DocumentViewModel>
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