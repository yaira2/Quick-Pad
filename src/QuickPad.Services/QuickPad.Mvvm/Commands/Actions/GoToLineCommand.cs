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
                viewModel.GoToLine(viewModel.LineToGoTo);
                viewModel.SetFocus();

                return Task.CompletedTask;
            };
        }
    }
}