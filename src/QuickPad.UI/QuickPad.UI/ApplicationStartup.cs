using Windows.UI.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Commands.Clipboard;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
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
            services.AddTransient<WelcomeDialog, WelcomeDialog>();
            services.AddTransient<GoToLine, GoToLine>();
            services.AddSingleton(provider => new QuickPadCommands(provider.GetService<PasteCommand>()));
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddTransient<IFindAndReplaceView, FindAndReplaceViewModel>();
            services.AddSingleton<SettingsViewModel, SettingsViewModel>();
            services.AddSingleton<DefaultTextForegroundColor, DefaultTextForegroundColor>();
            services.AddSingleton(_ => Application.Current as IApplication);
            services.AddSingleton<ApplicationController, ApplicationController>();
            services.AddSingleton<IVisualThemeSelector, VisualThemeSelector>();
            services.AddSingleton<MainPage, MainPage>();

            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}
