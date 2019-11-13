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
using QuickPad.Mvvm;
using Windows.UI.Popups;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace QuickPad.Mvc
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

            documentView.ViewModel.NewCommand = new SimpleCommand<DocumentViewModel> { Executioner = NewDocument };
            documentView.ViewModel.LoadCommand = new SimpleCommand<DocumentViewModel> { Executioner = LoadDocument };
            documentView.ViewModel.SaveCommand = new SimpleCommand<DocumentViewModel> { Executioner = SaveDocument };
            documentView.ViewModel.SaveAsCommand = new SimpleCommand<DocumentViewModel> { Executioner = SaveAsDocument };
            documentView.ViewModel.ExitCommand = new SimpleCommand<DocumentViewModel> { Executioner = ExitApplication };

            documentView.ViewModel.Initialize = viewModel =>
            {
                viewModel.HoldUpdates();
                
                viewModel.File = null;
                viewModel.Text = "";
                viewModel.IsDirty = false;
                viewModel.Encoding = Encoding.UTF8;
                
                viewModel.ReleaseUpdates();

                viewModel.NotifyAll();
            };
        }

        private Task NewDocument(DocumentViewModel documentViewModel)
        {
            documentViewModel.Initialize(documentViewModel);
            
            return Task.CompletedTask;
        }

        private async Task ExitApplication(DocumentViewModel documentViewModel)
        {
            if (documentViewModel.IsDirty)
            {
                await AskSaveDocument(documentViewModel);                
            }
            else
            {
                ExitApplication();
            }
        }

        private void ExitApplication()
        {
            Application.Current.Exit();
        }

        private async Task AskSaveDocument(DocumentViewModel documentViewModel)
        {
            if (!documentViewModel.IsDirty) return;

            var title = "Pending changes";
            var content = "There are pending changes.\r\nDo you want to save the modifications?";

            var yesCommand = new UICommand("Yes", async cmd => { await SaveDocument(documentViewModel); });
            var noCommand = new UICommand("No", cmd => { ExitApplication(); });
            var cancelCommand = new UICommand("Cancel", cmd => {  });

            var dialog = new MessageDialog(content, title);
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            if (noCommand != null)
            {
                dialog.Commands.Add(noCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            if (cancelCommand != null)
            {
                // Devices with a hardware back button
                // use the hardware button for Cancel.
                // for other devices, show a third option

                var t_hardwareBackButton = "Windows.Phone.UI.Input.HardwareButtons";

                if (ApiInformation.IsTypePresent(t_hardwareBackButton))
                {
                    // disable the default Cancel command index
                    // so that dialog.ShowAsync() returns null
                    // in that case

                    dialog.CancelCommandIndex = UInt32.MaxValue;
                }
                else
                {
                    dialog.Commands.Add(cancelCommand);
                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                }
            }

            var command = await dialog.ShowAsync();

            if (command == null && cancelCommand != null)
            {
                // back button was pressed
                // invoke the UICommand

                cancelCommand.Invoked(cancelCommand);
            }
        }

        private async Task LoadDocument(DocumentViewModel documentViewModel)
        {
            documentViewModel.HoldUpdates();

            var loadPicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };

            loadPicker.FileTypeFilter.Add(".txt");
            loadPicker.FileTypeFilter.Add(".rtf");
            loadPicker.FileTypeFilter.Add("*");

            var file = await loadPicker.PickSingleFileAsync();

            if (file == null) return;

            var provider = new FileDataProvider();
            var reader = new EncodingReader();
            reader.AddBytes(await provider.LoadDataAsync(file));
            var text = reader.Read(documentViewModel.Encoding);

            documentViewModel.File = file;
            documentViewModel.Text = text;
            documentViewModel.IsDirty = false;

            documentViewModel.ReleaseUpdates();
        }


        private Task SaveDocument(DocumentViewModel documentViewModel) => SaveDocument(documentViewModel, false);
        private Task SaveAsDocument(DocumentViewModel documentViewModel) => SaveDocument(documentViewModel, true);

        private async Task SaveDocument(DocumentViewModel documentViewModel, bool saveAs)
        {
            documentViewModel.HoldUpdates();

            var writer = new EncodingWriter();
            writer.Write(documentViewModel.Text);

            if (documentViewModel.File == null || saveAs)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Any", new List<string>() { "." });

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = documentViewModel.File.DisplayName ?? "Unnamed";

                var file = await savePicker.PickSaveFileAsync();

                if (file == null) return;
                documentViewModel.File = file;
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug($"Saving {documentViewModel.File.DisplayName}:\n{documentViewModel.Text}");
            }

            await new FileDataProvider().SaveDataAsync(documentViewModel.File, writer, documentViewModel.Encoding);

            documentViewModel.IsDirty = false;

            documentViewModel.ReleaseUpdates();
        }
    }
}
