using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowMenusCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowMenusCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.CurrentMode = "Classic Mode";

                return Task.CompletedTask;
            };
        }
    }
}