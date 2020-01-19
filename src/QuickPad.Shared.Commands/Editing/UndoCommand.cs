using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class UndoCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public UndoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.CanUndo;

            Executioner = viewModel =>
            {
                viewModel.RequestUndo();
                
                return Task.CompletedTask;
            };
        }
    }
}