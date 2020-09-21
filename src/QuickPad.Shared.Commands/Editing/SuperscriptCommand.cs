using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class SuperscriptCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public SuperscriptCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();

                viewModel.Document.SelSuperscript = !viewModel.Document.SelSuperscript;

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}