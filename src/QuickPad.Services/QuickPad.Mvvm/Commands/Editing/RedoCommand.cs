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
                viewModel.RequestRedo();

                return Task.CompletedTask;
            };
        }
    }
}