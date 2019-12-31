using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
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
                    documentView.GainedFocus += DocumentViewOnGainedFocus;

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

        private void DocumentViewOnGainedFocus(RoutedEventArgs obj)
        {
            if (!DataTransferManager.IsSupported()) return;

            try
            {
                var dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            }
            catch
            {
                // Will fail if in the background.
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (!(this._views.FirstOrDefault(v => v is IDocumentView) is IDocumentView view))
            {
                args.Request.FailWithDisplayText("Could not find IDocumentView - very bad.");
                return;
            }

            var request = args.Request;

            var viewModel = view.ViewModel;

            request.Data.Properties.ApplicationName = "Quick-Pad";

            if (viewModel.File == null)
            {
                request.Data.Properties.Title = $"Sharing {(viewModel.IsRtf ? "Rich Text" : "Text")}";

                request.Data.SetText(viewModel.Text);

                if (viewModel.IsRtf)
                {
                    request.Data.SetRtf(viewModel.RtfText);
                }
            }
            else
            {
                request.Data.Properties.Title = $"Sharing {viewModel.File.DisplayName}";
                request.Data.SetStorageItems(new []{viewModel.File});
            }

            request.Data.Properties.FileTypes.Add(viewModel.CurrentFileType);
            request.Data.Properties.Description = "Shared from Quick-Pad\n\n";
        }
    }
}