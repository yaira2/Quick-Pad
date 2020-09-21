using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Commands.Actions;
using QuickPad.Mvvm.Managers;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Commands;
using QuickPad.UI.Commands.Clipboard;
using QuickPad.UI.Dialogs;
using QuickPad.UI.Helpers;
using QuickPad.UI.Theme;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace QuickPad.UI
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // MVC and ViewModels
            services.AddSingleton<ApplicationController<StorageFile, IRandomAccessStream, WindowsDocumentManager>, ApplicationController<StorageFile, IRandomAccessStream, WindowsDocumentManager>>();
            services.AddSingleton<IApplication<StorageFile, IRandomAccessStream>>(provider => App.Current as IApplication<StorageFile, IRandomAccessStream>);
            services.AddSingleton<SettingsViewModel<StorageFile, IRandomAccessStream>>(provider => provider.GetService<WindowsSettingsViewModel>());
            services.AddSingleton<WindowsSettingsViewModel, WindowsSettingsViewModel>();
            services.AddSingleton<WindowsSettingsModel, WindowsSettingsModel>();
            services.AddTransient<DocumentViewModel<StorageFile, IRandomAccessStream>, DocumentViewModel<StorageFile, IRandomAccessStream>>();
            services.AddSingleton<DefaultTextForegroundColor, DefaultTextForegroundColor>();
            services.AddSingleton<RtfDocumentOptions, RtfDocumentOptions>();
            services.AddSingleton<TextDocumentOptions, TextDocumentOptions>();
            services.AddTransient<IFindAndReplaceView<StorageFile, IRandomAccessStream>, FindAndReplaceViewModel<StorageFile, IRandomAccessStream>>();

            services.AddTransient<RtfDocument>(provider =>
            {
                var options = provider.GetService<RtfDocumentOptions>();

                return new RtfDocument(
                    options.Document
                    , provider
                    , options.Logger
                    , options.ViewModel
                    , provider.GetService<WindowsSettingsViewModel>()
                    , provider.GetService<IApplication<StorageFile, IRandomAccessStream>>());
            });

            services.AddTransient<TextDocument>(provider =>
            {
                var options = provider.GetService<TextDocumentOptions>();

                return new TextDocument(
                    options.Document
                    , options.Logger
                    , options.ViewModel
                    , provider.GetService<WindowsSettingsViewModel>()
                    , provider.GetService<IApplication<StorageFile, IRandomAccessStream>>());
            });

            services.AddTransient<DocumentModel<StorageFile, IRandomAccessStream>>(provider =>
            {
                var vm = provider.GetService<DocumentViewModel<StorageFile, IRandomAccessStream>>();

                if (vm != null)
                {
                    switch (vm.CurrentFileType.ToLowerInvariant().Trim('.'))
                    {
                        case "rtf":
                            return provider.GetService<RtfDocument>();
                    }
                }

                return provider.GetService<TextDocument>();
            });
            services.AddSingleton<WindowsDocumentManager, WindowsDocumentManager>();
            services.AddSingleton<IDocumentViewModelStrings, WindowsDocumentViewModelStrings>();
            services.AddTransient<ResourceLoader>(provider => ResourceLoader.GetForCurrentView());
            services.AddTransient<ITextDocument>(provider => App.RichEditBox?.Document);

            services.AddSingleton<IVisualThemeSelector, VisualThemeSelector>();

            services.AddSingleton<DialogManager, DialogManager>();
            services.AddTransient<AskToSave, AskToSave>();
            services.AddTransient<AskForReviewDialog, AskForReviewDialog>();
            services.AddTransient<IGoToLineView<StorageFile, IRandomAccessStream>, GoToLine>();
            services.AddSingleton<MainPage, MainPage>();

            services.AddSingleton<IShowGoToCommand<StorageFile, IRandomAccessStream>, ShowGoToCommand<StorageFile, IRandomAccessStream>>();
            services.AddSingleton<IShareCommand<StorageFile, IRandomAccessStream>, ShareCommand>();
            services.AddSingleton<ICutCommand<StorageFile, IRandomAccessStream>, CutCommand>();
            services.AddSingleton<ICopyCommand<StorageFile, IRandomAccessStream>, CopyCommand>();
            services.AddSingleton<IPasteCommand<StorageFile, IRandomAccessStream>, PasteCommand>();
            services.AddSingleton<IDeleteCommand<StorageFile, IRandomAccessStream>, DeleteCommand>();
            services.AddSingleton<IContentChangedCommand<StorageFile, IRandomAccessStream>, ContentChangedCommand>();
            services.AddSingleton<IEmojiCommand<StorageFile, IRandomAccessStream>, EmojiCommand>();
            services.AddSingleton<ICompactOverlayCommand<StorageFile, IRandomAccessStream>, CompactOverlayCommand>();
            services.AddSingleton<IRateAndReviewCommand<StorageFile, IRandomAccessStream>, RateAndReviewCommand>();
            services.AddSingleton<QuickPadCommands<StorageFile, IRandomAccessStream>, QuickPadCommands<StorageFile, IRandomAccessStream>>();
            services.AddSingleton<IQuickPadCommands<StorageFile, IRandomAccessStream>>(provider => provider.GetService<QuickPadCommands<StorageFile, IRandomAccessStream>>());
            services.AddSingleton<PasteCommand, PasteCommand>();

            services.AddSingleton(_ => Application.Current as IApplication<StorageFile, IRandomAccessStream>);
            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}