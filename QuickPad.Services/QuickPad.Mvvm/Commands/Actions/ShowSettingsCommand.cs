using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowSettingsCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowSettingsCommand(IServiceProvider serviceProvider)
        {
            Executioner = viewModel =>
            {
                var settings = serviceProvider.GetService<SettingsViewModel>();

                //open settings page
                settings.ShowSettings = !settings.ShowSettings;

                return Task.CompletedTask;
            };
        }
    }

    public class ShowCommandBarCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowCommandBarCommand(IServiceProvider serviceProvider)
        {
            Executioner = viewModel =>
            {
                var settings = serviceProvider.GetService<SettingsViewModel>();

                //open settings page
                settings.CurrentMode = "Default";

                return Task.CompletedTask;
            };
        }
    }

    public class ShowMenusCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowMenusCommand(IServiceProvider serviceProvider)
        {
            Executioner = viewModel =>
            {
                var settings = serviceProvider.GetService<SettingsViewModel>();

                //open settings page
                settings.CurrentMode = "Classic Mode";

                return Task.CompletedTask;
            };
        }
    }

}
