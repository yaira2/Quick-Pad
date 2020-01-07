using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowGoToCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowGoToCommand(IServiceProvider provider)
        {
            Executioner = viewModel =>
            {
                var dialog = provider.GetService<IGoToLineView>();
                dialog.ViewModel = viewModel;
                viewModel.LineToGoTo = viewModel.CurrentLine;
                _ = dialog.ShowAsync();

                return Task.CompletedTask;
            };
        }
    }
}