using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class UndoCommand : SimpleCommand<DocumentViewModel>
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