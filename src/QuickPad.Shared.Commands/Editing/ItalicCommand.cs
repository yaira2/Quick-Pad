using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class ItalicCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ItalicCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();
                viewModel.Document.SelItalic = !viewModel.Document.SelItalic;
                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}