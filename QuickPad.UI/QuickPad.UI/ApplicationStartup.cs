using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvc;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Commands.Actions;
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
            services.AddSingleton<ShowCommandBarCommand, ShowCommandBarCommand>();
            services.AddSingleton<ShowMenusCommand, ShowMenusCommand>();
            services.AddSingleton<FocusCommand, FocusCommand>();
            services.AddSingleton(provider => new QuickPadCommands(provider.GetService<PasteCommand>()));
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddSingleton<SettingsViewModel, SettingsViewModel>();
            services.AddSingleton<ApplicationController, ApplicationController>();
            services.AddTransient<VisualThemeSelector, VisualThemeSelector>();
            services.AddSingleton<MainPage, MainPage>();

            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}
