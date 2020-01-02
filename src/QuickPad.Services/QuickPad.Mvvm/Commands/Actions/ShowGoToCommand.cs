using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowGoToCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowGoToCommand()
        {
            //var dialog = provider.GetService<GoToLine>();
            //_ = dialog.ShowAsync();
        }
    }
}