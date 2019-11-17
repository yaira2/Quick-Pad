using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm;
using QuickPad.MVVM;
using QuickPad.UI.Common;

namespace QuickPad.Mvc
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
