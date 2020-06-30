using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class RedoCommand : SimpleCommand<DocumentViewModel>
    {
        public RedoCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.CanRedo;

            Executioner = viewModel =>
            {
                if (viewModel.CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    viewModel.Document.Redo(); //undo changes the user did to the text            

                    viewModel.OnPropertyChanged(nameof(viewModel.Text));
                }
                else
                {
                    viewModel.RequestRedo();
                }

                return Task.CompletedTask;
            };
        }
    }
}