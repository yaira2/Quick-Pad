using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickPad.Data;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Managers;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.Standard.Data;
using QuickPad.UI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace QuickPad.UI.Helpers
{
    public class WindowsDocumentManager : DocumentManager<StorageFile, IRandomAccessStream, WindowsDocumentManager>
    {
        protected override void DocumentViewOnGainedFocus()
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
            DocumentViewModel<StorageFile, IRandomAccessStream> first = App.CurrentViewModel;

            if (!(first is DocumentViewModel<StorageFile, IRandomAccessStream> viewModel))
            {
                args.Request.FailWithDisplayText("Could not find DocumentViewModel - very bad.");
                return;
            }

            var request = args.Request;

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
                request.Data.SetStorageItems(new[] { viewModel.File.File as StorageFile });
            }

            request.Data.Properties.FileTypes.Add(viewModel.CurrentFileType);
            request.Data.Properties.Description = "Shared from Quick-Pad\n\n";
        }

        protected override async Task<bool> DocumentViewExitApplication(
            DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            return await ExitApp(documentViewModel) != SaveState.Canceled;
        }

        public override void Initializer(IDocumentView<StorageFile, IRandomAccessStream> documentView
            , IQuickPadCommands<StorageFile, IRandomAccessStream> commands
            , IApplication<StorageFile, IRandomAccessStream> app)
        {
            App = app;

            commands.NewDocumentCommandBase.Executioner = NewDocument;
            commands.LoadCommandBase.Executioner = LoadDocument;
            commands.SaveCommandBase.Executioner = async documentViewModel => await SaveDocument(documentViewModel);
            commands.SaveAsCommandBase.Executioner = SaveAsDocument;
            commands.ExitCommandBase.Executioner = ExitApplication;

            if (documentView.ViewModel == null)
            {
                documentView.ViewModel =
                    ServiceProvider.GetService<DocumentViewModel<StorageFile, IRandomAccessStream>>();
                documentView.ViewModel.Initialize = viewModel => viewModel.InitNewDocument();
            }
        }

        public override async Task NewDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            var canceled = false;
            if (documentViewModel.Document.IsDirty)
            {
                var saved = await AskSaveDocument(documentViewModel, false);
                canceled = saved == SaveState.Canceled;
            }

            if (!canceled)
            {
                documentViewModel.Initialize = viewModel => viewModel.InitNewDocument();

                documentViewModel.Initialize(documentViewModel);

                Settings.Status("New document initialized.", TimeSpan.FromSeconds(10),
                    Verbosity.Debug);
            }
        }

        public override void SaveDocument(IDocumentView<StorageFile, IRandomAccessStream> obj)
        {
            App.TryEnqueue(() =>
                SaveDocument(obj.ViewModel));
        }

        public override void SaveDocumentToCache(IDocumentView<StorageFile, IRandomAccessStream> obj, string cacheFilename)
        {
            App.TryEnqueue(async () => await SaveSilently(obj.ViewModel, cacheFilename));
        }

        protected override Task ExitApplication(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            return ExitApp(documentViewModel);
        }

        protected override Task<SaveState> ExitApp(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            Settings.ShowSettings = false;

            return documentViewModel.Document.IsDirty
                ? AskSaveDocument(documentViewModel, true)
                : Task.FromResult(Close(documentViewModel) == DeferredState.Deferred ? SaveState.Saved : SaveState.Unsaved);
        }

        protected override DeferredState Close(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            if (documentViewModel.Deferral != null)
            {
                Settings.NotDeferred = false;
                documentViewModel.Deferred = true;
                try
                {
                    ((Deferral)documentViewModel.Deferral).Complete();
                    return DeferredState.Deferred;
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                Settings.NotDeferred = true;
                documentViewModel.Deferred = false;

                Settings.ExitApplication?.Invoke();
            }

            return DeferredState.NotDeferred;
        }

        protected override async Task<SaveState> AskSaveDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel, bool isClosing = true)
        {
            if (!documentViewModel.Document.IsDirty && isClosing)
                return (Close(documentViewModel) == DeferredState.Deferred
                    ? SaveState.DeferredSaved
                    : SaveState.UnDeferredSaved);

            async Task<SaveState> Yes()
            {
                var saved = await SaveDocument(documentViewModel);

                return saved switch
                {
                    SaveState.Saved => (isClosing
                        ? (Close(documentViewModel) == DeferredState.Deferred
                            ? SaveState.DeferredSaved
                            : SaveState.UnDeferredSaved)
                        : SaveState.Saved),
                    SaveState.Unsaved => (isClosing
                        ? (Close(documentViewModel) == DeferredState.Deferred
                            ? SaveState.DeferredNotSaved
                            : SaveState.UnDeferredNotSaved)
                        : SaveState.Unsaved),
                    _ => saved
                };
            }

            var (success, dialog) = ServiceProvider.GetService<DialogManager>().RequestDialog<AskToSave>();

            if (!success) return SaveState.Canceled;

            dialog.ViewModel = documentViewModel;

            var result = await dialog.ShowAsync();

            return result switch
            {
                ContentDialogResult.Primary => await Yes(),
                ContentDialogResult.Secondary => (isClosing
                    ? (Close(documentViewModel) == DeferredState.Deferred
                        ? SaveState.DeferredNotSaved
                        : SaveState.UnDeferredNotSaved)
                    : SaveState.Unsaved),
                _ => SaveState.Canceled
            };
        }

        protected override async Task LoadDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            if (ServiceProvider.GetService<DialogManager>().CurrentDialogView != null)
            {
                Logger.LogCritical("There is already a dialog open.");
                return;
            }

            if (documentViewModel.Document.IsDirty)

            {
                if ((await AskSaveDocument(documentViewModel, false)) == SaveState.Canceled) return;
            }

            documentViewModel.HoldUpdates();

            var loadPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            switch (Settings.DefaultFileType.ToLowerInvariant())
            {
                case ".rtf":
                    loadPicker.FileTypeFilter.Add(".rtf");
                    loadPicker.FileTypeFilter.Add(".txt");
                    loadPicker.FileTypeFilter.Add("*");
                    break;

                case ".txt":
                    loadPicker.FileTypeFilter.Add(".txt");
                    loadPicker.FileTypeFilter.Add(".rtf");
                    loadPicker.FileTypeFilter.Add("*");
                    break;

                default:
                    loadPicker.FileTypeFilter.Add("*");
                    loadPicker.FileTypeFilter.Add(".rtf");
                    loadPicker.FileTypeFilter.Add(".txt");
                    break;
            }

            var file = await loadPicker.PickSingleFileAsync();

            if (file != null)
            {
                await LoadFile(documentViewModel, file);
            }
        }

        public override async Task LoadFile(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel, StorageFile file)
        {
            var canceled = false;
            if (documentViewModel.Document.IsDirty)
            {
                var saved = await AskSaveDocument(documentViewModel, false);
                canceled = saved == SaveState.Canceled;
            }

            if (!canceled)
            {
                documentViewModel.Document = file.FileType.ToLowerInvariant().TrimStart('.') switch
                {
                    "rtf" => (DocumentModel<StorageFile, IRandomAccessStream>)ServiceProvider.GetService<RtfDocument>(),
                    _ => (DocumentModel<StorageFile, IRandomAccessStream>)ServiceProvider.GetService<TextDocument>()
                };

                //add file to most recently used list
                var mru = StorageApplicationPermissions.MostRecentlyUsedList.Add(file, file.Name, RecentStorageItemVisibility.AppAndSystem);

                var provider = new StorageFileDataProvider();
                var bytes = await provider.LoadDataAsync(file);
                var reader = new EncodingReader();
                reader.AddBytes(bytes);

                documentViewModel.CurrentEncoding = Settings.DefaultEncoding switch
                {
                    "UTF-8" => Encoding.UTF8,
                    "Utf8" => Encoding.UTF8,
                    "UTF-16 LE" => Encoding.Unicode,
                    "UTF-16 BE" => Encoding.BigEndianUnicode,
                    "UTF-32" => Encoding.UTF32,
                    _ => Encoding.ASCII
                };

                byte[] bom = null;

                try
                {
                    ByteOrderMarks.ToList().ForEach(pair =>
                    {
                        var (key, value) = pair;

                        if (!bytes.AsSpan(0, value.Length).StartsWith(value.AsSpan(0, value.Length))) return;

                        var encoding = key switch
                        {
                            ByteOrderMark.Utf8 => Encoding.UTF8,
                            ByteOrderMark.Utf16Be => Encoding.BigEndianUnicode,
                            ByteOrderMark.Utf16Le => Encoding.Unicode,
                            ByteOrderMark.Utf32Be => Encoding.UTF32,
                            ByteOrderMark.Utf32Le => Encoding.UTF32,
                            ByteOrderMark.Utf7A => Encoding.UTF7,
                            ByteOrderMark.Utf7B => Encoding.UTF7,
                            ByteOrderMark.Utf7C => Encoding.UTF7,
                            ByteOrderMark.Utf7D => Encoding.UTF7,
                            ByteOrderMark.Utf7E => Encoding.UTF7,
                            _ => Encoding.ASCII
                        };

                        documentViewModel.CurrentEncoding = encoding;

                        reader = new EncodingReader();
                        reader.AddBytes(bytes.AsSpan().Slice(value.Length).ToArray());
                        bom = value;
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(new EventId(), $"Error loading {file.Name}.", ex);
                    Settings.Status(ex.Message, TimeSpan.FromSeconds(60), Verbosity.Error);
                }

                var text = reader.Read(documentViewModel.CurrentEncoding);
                documentViewModel.File = new UwpStorageFileWrapper { File = file, BOM = bom };

                if (text.Contains(Environment.NewLine))
                {
                    documentViewModel.File.OriginalLineEndings = Environment.NewLine;
                    documentViewModel.File.TargetLineEndings = Environment.NewLine;
                    text = text.Replace(Environment.NewLine, "\r");
                }

                // Ensure valid RTF
                if (documentViewModel.IsRtf
                    && !text.StartsWith("{\\rtf1", StringComparison.InvariantCultureIgnoreCase))
                {
                    text = "{\\rtf1" + text;
                }

                // Ensure valid RTF
                if (documentViewModel.IsRtf
                    && !text.EndsWith("}", StringComparison.InvariantCultureIgnoreCase))
                {
                    text += "}";
                }

                if (documentViewModel.IsRtf)
                {
                    var memoryStream = new InMemoryRandomAccessStream();
                    var b = documentViewModel.CurrentEncoding.GetBytes(text);
                    var buffer = b.AsBuffer();
                    await memoryStream.WriteAsync(buffer);

                    memoryStream.Seek(0);

                    try
                    {
                        documentViewModel.Document?.LoadFromStream(QuickPadTextSetOptions.FormatRtf, memoryStream);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error loading {file.Path}");
                    }
                }
                else
                {
                    documentViewModel.SetText(text.Replace(Environment.NewLine, "\r"), false);
                    documentViewModel.InvokeClearUndoRedo();
                }

                documentViewModel.ReleaseUpdates();

                Settings.Status($"Loaded {documentViewModel.File.Name}", TimeSpan.FromSeconds(5),
                    Verbosity.Release);
            }
        }

        protected override Task<SaveState> SaveDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            return SaveDocument(documentViewModel, false);
        }

        protected override Task<SaveState> SaveAsDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel)
        {
            return SaveDocument(documentViewModel, true);
        }

        protected override async Task<SaveState> SaveSilently(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel, string cacheFilename)
        {
            try
            {
                documentViewModel.HoldUpdates();

                // Create cache folder if does not exists
                StorageFolder cacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("cachedFiles", CreationCollisionOption.OpenIfExists);

                // Select the filename if null
                if (string.IsNullOrWhiteSpace(cacheFilename))
                {
                    cacheFilename = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.f"); // Precise date
                }

                // Create the file
                StorageFileWrapper<StorageFile> file = new UwpStorageFileWrapper() { File = await cacheFolder.CreateFileAsync(cacheFilename) };

                // Save it
                if (Logger.IsEnabled(LogLevel.Debug))
                    Logger.LogDebug($"Saving {file.DisplayName}:\n{documentViewModel.Text}");

                EncodingWriter writer = new EncodingWriter();

                string textToSave = documentViewModel.IsRtf switch
                {
                    true => FixLineEndings(documentViewModel.RtfText),
                    _ => FixLineEndings(documentViewModel.Text)
                };

                string FixLineEndings(string text)
                {
                    return !text.Contains(file.TargetLineEndings)
                        ? text.Replace("\r", file.TargetLineEndings)
                        : text;
                }

                await App.AwaitableRunAsync(() => writer.Write(textToSave));

                try
                {
                    await new StorageFileDataProvider().SaveDataAsync(file, writer,
                        documentViewModel.CurrentEncoding);

                    documentViewModel.Document.IsUnsavedCache = false;
                    documentViewModel.Document.CacheFilename = cacheFilename;

                    Settings.Status($"Saved {file.Name}", TimeSpan.FromSeconds(10),
                        Verbosity.Release);

                    return SaveState.Saved;
                }
                catch (Exception e)
                {
                    Settings.Status(e.Message, TimeSpan.FromSeconds(10), Verbosity.Error);

                    return SaveState.Canceled;
                }
            }
            finally
            {
                documentViewModel.ReleaseUpdates();
            }
        }

        protected override async Task<SaveState> SaveDocument(DocumentViewModel<StorageFile, IRandomAccessStream> documentViewModel, bool saveAs)
        {
            if (ServiceProvider.GetService<DialogManager>().CurrentDialogView != null) return SaveState.Canceled;

            documentViewModel.HoldUpdates();

            if (documentViewModel.File == null || saveAs)
            {
                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                if (documentViewModel.CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string> { ".rtf" });
                    savePicker.FileTypeChoices.Add("Plain Text", new List<string> { ".txt" });
                    savePicker.FileTypeChoices.Add("Any", new List<string> { "." });
                }
                else if (documentViewModel.CurrentFileType.Equals(".txt",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    savePicker.FileTypeChoices.Add("Plain Text", new List<string> { ".txt" });
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string> { ".rtf" });
                    savePicker.FileTypeChoices.Add("Any", new List<string> { "." });
                }
                else
                {
                    savePicker.FileTypeChoices.Add("Document",
                        new List<string> { documentViewModel.CurrentFileType });
                    savePicker.FileTypeChoices.Add("Plain Text", new List<string> { ".txt" });
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string> { ".rtf" });
                    savePicker.FileTypeChoices.Add("Any", new List<string> { "." });
                }
                // Dropdown of file types the user can save the file as

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = documentViewModel.File?.DisplayName ?? DocumentViewModel<StorageFile, IRandomAccessStream>.Untitled;

                var file = await savePicker.PickSaveFileAsync();

                //add file to most recently used list
                var mru = StorageApplicationPermissions.MostRecentlyUsedList.Add(file, file.Name, RecentStorageItemVisibility.AppAndSystem);

                if (file == null) return SaveState.Canceled;
                documentViewModel.File = new UwpStorageFileWrapper { File = file };
            }

            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug($"Saving {documentViewModel.File.DisplayName}:\n{documentViewModel.Text}");

            var writer = new EncodingWriter();

            var textToSave = documentViewModel.IsRtf switch
            {
                true => FixLineEndings(documentViewModel.RtfText),
                _ => FixLineEndings(documentViewModel.Text)
            };

            string FixLineEndings(string text)
            {
                return !text.Contains(documentViewModel.File.TargetLineEndings)
                    ? text.Replace("\r", documentViewModel.File.TargetLineEndings)
                    : text;
            }

            await App.AwaitableRunAsync(() => writer.Write(textToSave));

            try
            {
                await new StorageFileDataProvider().SaveDataAsync(documentViewModel.File, writer,
    documentViewModel.CurrentEncoding);

                documentViewModel.Document.IsDirty = false;

                documentViewModel.ReleaseUpdates();

                Settings.Status($"Saved {documentViewModel.File.Name}", TimeSpan.FromSeconds(10),
                    Verbosity.Release);

                return SaveState.Saved;
            }
            catch (Exception e)
            {
                Settings.Status(e.Message, TimeSpan.FromSeconds(10), Verbosity.Error);

                return SaveState.Canceled;
            }
        }
    }

    public class UwpStorageFileWrapper : StorageFileWrapper<StorageFile>
    {
        public override string FileType => File.FileType;
        public override string DisplayType => File.DisplayType;
        public override string DisplayName => File.DisplayName;
        public override string Path => File.Path;
        public override string Name => File.Name;

        public byte[] BOM
        {
            get;
            internal set;
        }
    }
}