using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.MVVM.Commands.Actions
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

}
