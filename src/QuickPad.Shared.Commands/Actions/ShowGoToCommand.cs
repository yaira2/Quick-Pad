using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Managers;
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
                var (status, dialog) = provider.GetService<DialogManager>().RequestDialog<IGoToLineView<TStorageFile, TStream>>();

                if (!status) return Task.FromException(new ApplicationException("There is already an open dialog."));

                dialog.ViewModel = viewModel;
                viewModel.LineToGoTo = viewModel.CurrentLine;
                _ = dialog.ShowAsyncByTask();

                return Task.CompletedTask;
            };
        }
    }
}