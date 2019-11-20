using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowCommandBarCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowCommandBarCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.CurrentMode = "Default";

                return Task.CompletedTask;
            };
        }
    }
}