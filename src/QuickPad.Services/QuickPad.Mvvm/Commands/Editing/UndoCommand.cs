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
                if (viewModel.CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    viewModel.Document.Undo(); //undo changes the user did to the text            

                    viewModel.OnPropertyChanged(nameof(viewModel.Text));
                }
                else
                {
                    viewModel.RequestUndo();
                }

                return Task.CompletedTask;
            };
        }
    }
}