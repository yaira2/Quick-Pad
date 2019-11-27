using Windows.UI.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Commands.Actions;
using QuickPad.Mvvm.Commands.Clipboard;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Common;
using QuickPad.UI.Common.Dialogs;
using QuickPad.UI.Common.Theme;

namespace QuickPad.UI
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PasteCommand, PasteCommand>();
            services.AddTransient<AskToSave, AskToSave>();
            services.AddSingleton(provider => new QuickPadCommands(provider.GetService<PasteCommand>()));
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddTransient<IFindAndReplaceView, FindAndReplaceViewModel>();
            services.AddSingleton<SettingsViewModel, SettingsViewModel>();
            services.AddSingleton(_ => Application.Current as IApplication);
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
