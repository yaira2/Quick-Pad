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
using QuickPad.Mvvm.Commands.Actions;
using Windows.ApplicationModel.Resources;
using Windows.UI;

namespace QuickPad.UI
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PasteCommand, PasteCommand>();
            services.AddTransient<AskToSave, AskToSave>();
            services.AddTransient<ShowGoToCommand, ShowGoToCommand>();
            services.AddSingleton(provider => new QuickPadCommands(
                provider.GetService<PasteCommand>(),
                provider.GetService<ShowGoToCommand>()));
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddTransient<IFindAndReplaceView, FindAndReplaceViewModel>();
            services.AddSingleton<SettingsViewModel, SettingsViewModel>();
            services.AddSingleton<DefaultTextForegroundColor, DefaultTextForegroundColor>();
            services.AddSingleton(_ => Application.Current as IApplication);
            services.AddSingleton<ApplicationController, ApplicationController>();
            services.AddSingleton<IVisualThemeSelector, VisualThemeSelector>();
            services.AddTransient<IGoToLineView, GoToLine>();
            services.AddSingleton<MainPage, MainPage>();
            services.AddSingleton<ResourceLoader>(provider =>
                (ResourceLoader)(Application.Current.Resources.TryGetValue(nameof(ResourceLoader), out var value) ? value : null));

            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}
