using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.MVC;
using QuickPad.MVVM.Commands;
using QuickPad.MVVM.Commands.Clipboard;
using QuickPad.MVVM.ViewModels;
using QuickPad.UI.Common;

namespace QuickPad.UI
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PasteCommand, PasteCommand>();
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
