using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowGoToCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>, IShowGoToCommand<TStorageFile, TStream>
        where TStream : class
    {
        public ShowGoToCommand(IServiceProvider provider)
        {
            Executioner = viewModel =>
            {
                var dialog = provider.GetService<IGoToLineView<TStorageFile, TStream>>();
                dialog.ViewModel = viewModel;
                viewModel.LineToGoTo = viewModel.CurrentLine;
                _ = dialog.ShowAsyncByTask();

                return Task.CompletedTask;
            };
        }
    }
}