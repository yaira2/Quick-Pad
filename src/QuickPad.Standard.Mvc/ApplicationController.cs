using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using System;
using System.Collections.Generic;

namespace QuickPad.Mvc
{
    public partial class ApplicationController<TStorageFile, TStream, TDocumentManager>
        where TDocumentManager : DocumentManager<TStorageFile, TStream, TDocumentManager>
        where TStream : class
    {
        private readonly TDocumentManager _documentManager;
        private readonly List<IView> _views = new List<IView>();

        public ApplicationController(
            ILogger<ApplicationController<TStorageFile, TStream, TDocumentManager>> logger
            , IServiceProvider serviceProvider
            , SettingsViewModel<TStorageFile, TStream> settings
            , TDocumentManager documentManager)
        {
            _documentManager = documentManager;
            Logger = logger;
            documentManager.ServiceProvider = ServiceProvider = serviceProvider;
            documentManager.Settings = Settings = settings;
            documentManager.Logger = Logger = logger;

            if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Started Application Controller.");
        }

        internal ILogger<ApplicationController<TStorageFile, TStream, TDocumentManager>> Logger { get; }
        internal IServiceProvider ServiceProvider { get; }

        internal SettingsViewModel<TStorageFile, TStream> Settings { get; set; }

        public void AddView<TView>(TView view) where TView : IView
        {
            if (_views.Contains(view)) return;

            switch (view)
            {
                case IDocumentView<TStorageFile, TStream> documentView:

                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Added IDocumentView to controller.");

                    _views.Add(view);

                    documentView.Initialize += _documentManager.Initializer;
                    documentView.ExitApplication += _documentManager.DocumentViewExitApplication;
                    documentView.LoadFromFile += _documentManager.LoadFile;
                    documentView.GainedFocus += _documentManager.DocumentViewOnGainedFocus;
                    documentView.SaveToFile += _documentManager.SaveDocument;
                    documentView.SaveToCache += _documentManager.SaveDocumentToCache;

                    _documentManager.Initializer(documentView
                        , ServiceProvider.GetService<IQuickPadCommands<TStorageFile, TStream>>()
                        , ServiceProvider.GetService<IApplication<TStorageFile, TStream>>());

                    break;

                case IFindAndReplaceView<TStorageFile, TStream> findAndReplaceView:
                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Added IFindAndReplaceView to controller.");

                    _views.Add(view);

                    findAndReplaceView.SearchNext += FindAndReplaceManager<TStorageFile, TStream>.SearchNext;
                    findAndReplaceView.SearchPrevious += FindAndReplaceManager<TStorageFile, TStream>.SearchPrevious;
                    findAndReplaceView.SearchReplaceNext += FindAndReplaceManager<TStorageFile, TStream>.ReplaceNext;
                    findAndReplaceView.SearchReplaceAll += FindAndReplaceManager<TStorageFile, TStream>.ReplaceAll;

                    break;
            }
        }
    }
}