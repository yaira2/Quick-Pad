using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickPad.Data;
using QuickPad.MVVM;

namespace QuickPad.MVC
{
    public class ApplicationController 
    {
        private readonly List<IView> _views = new List<IView>();

        private ILogger<ApplicationController> Logger { get; }
        public IServiceProvider ServiceProvider { get; }
        
        public ApplicationController(ILogger<ApplicationController> logger, IServiceProvider serviceProvider)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Started Application Controller.");
            }
        }

        public void AddView<TView>(TView view) where TView : IView
        {
            if (_views.Contains(view)) return;

            switch (view)
            {
                case IDocumentView documentView:

                    if (documentView.ViewModel != null) return;

                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug("Added IDocumentView to controller.");
                    }

                    _views.Add(view);

                    documentView.Initialize += DocumentInitializer;

                    break;
            }
        }

        private void DocumentInitializer(IDocumentView documentView)
        {
            documentView.ViewModel = ServiceProvider.GetService<DocumentViewModel>();

            documentView.ViewModel.SaveCommand = new SimpleCommand<DocumentViewModel> { Executioner = SaveDocument };

            documentView.ViewModel.Initialize = viewModel =>
            {
                viewModel.Text = "New Document."; 
                viewModel.Encoding = Encoding.UTF8;
            };
        }

        private async Task SaveDocument(DocumentViewModel documentViewModel)
        {
            var writer = new EncodingWriter();
            writer.Write(documentViewModel.Text);

            if (documentViewModel.File == null)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "New Document";
                
                var file = await savePicker.PickSaveFileAsync();

                if (file == null) return;
                documentViewModel.File = file;
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug($"Saving {documentViewModel.File.DisplayName}:\n{documentViewModel.Text}");
            }

            await new FileDataProvider().SaveDataAsync(documentViewModel.File, writer, documentViewModel.Encoding);
        }
    }
}
