using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvc;
using QuickPad.Mvvm.Commands;
using QuickPad.MVVM.Commands.Actions;
using QuickPad.Mvvm.Commands.Clipboard;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;

namespace QuickPad.UI
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PasteCommand, PasteCommand>();
            services.AddSingleton<ShowSettingsCommand, ShowSettingsCommand>();
            services.AddSingleton<QuickPadCommands, QuickPadCommands>();
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddSingleton<SettingsViewModel, SettingsViewModel>();
            services.AddSingleton<ApplicationController, ApplicationController>();
            services.AddTransient<VisualThemeSelector, VisualThemeSelector>();

            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}
