using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvc
{
    public partial class ApplicationController
    {
        private readonly List<IView> _views = new List<IView>();

        public ApplicationController(ILogger<ApplicationController> logger, IServiceProvider serviceProvider, SettingsViewModel settings)
        {
            Logger = logger;
            DocumentManager.ServiceProvider = ServiceProvider = serviceProvider;
            DocumentManager.Settings = Settings = settings;
            DocumentManager.Logger = Logger = logger;

            if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Started Application Controller.");
        }

        internal ILogger<ApplicationController> Logger { get; }
        internal IServiceProvider ServiceProvider { get; }

        internal SettingsViewModel Settings { get; set; }

        public void AddView<TView>(TView view) where TView : IView
        {
            if (_views.Contains(view)) return;

            switch (view)
            {
                case IDocumentView documentView:

                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Added IDocumentView to controller.");

                    _views.Add(view);

                    documentView.Initialize += DocumentManager.Initializer;
                    documentView.ExitApplication += DocumentManager.DocumentViewExitApplication;
                    documentView.LoadFromFile += DocumentManager.LoadFile;

                    break;

                case IFindAndReplaceView findAndReplaceView:
                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Added IFindAndReplaceView to controller.");

                    _views.Add(view);

                    findAndReplaceView.SearchNext += FindAndReplaceManager.SearchNext;
                    findAndReplaceView.SearchPrevious += FindAndReplaceManager.SearchPrevious;
                    findAndReplaceView.SearchReplaceNext += FindAndReplaceManager.ReplaceNext;
                    findAndReplaceView.SearchReplaceAll += FindAndReplaceManager.ReplaceAll;

                    break;
            }
        }
    }
}