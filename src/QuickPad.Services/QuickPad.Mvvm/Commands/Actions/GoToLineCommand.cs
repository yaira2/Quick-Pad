using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class GoToLineCommand : SimpleCommand<DocumentViewModel>
    {
        public GoToLineCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();

                viewModel.Document.Selection.StartPosition = viewModel.LineToGoTo;

                viewModel.Document.EndUndoGroup();

                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}