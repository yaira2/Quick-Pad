using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class SubscriptCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public SubscriptCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();

                viewModel.Document.SelSubscript = !viewModel.Document.SelSubscript;

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}