using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickPad.Data;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Common.Dialogs;

namespace QuickPad.Mvc
{
    public partial class ApplicationController
    {
        private static class DocumentManager
        {
            private const string HARDWARE_BACK_BUTTON = "Windows.Phone.UI.Input.HardwareButtons";
            private const string RTF_MARKER = "{\\rtf1";

            internal static ILogger<ApplicationController> Logger { get; set; }
            internal static IServiceProvider ServiceProvider { get; set; }

            private static readonly Dictionary<ByteOrderMark, byte[]> _byteOrderMarks = 
                new Dictionary<ByteOrderMark, byte[]>
            {
                {ByteOrderMark.Utf8, new byte[] {0xEF, 0xBB, 0xBF}},
                {ByteOrderMark.Utf16Be, new byte[] {0xFE, 0xFF}},
                {ByteOrderMark.Utf16Le, new byte[] {0xFF, 0xFE}},
                {ByteOrderMark.Utf32Be, new byte[] {0x00, 0x00, 0xFE, 0xFF}},
                {ByteOrderMark.Utf32Le, new byte[] {0xFF, 0xFE, 0x00, 0x00}},
                {ByteOrderMark.Utf7A, new byte[] {0x2B, 0x2F, 0x76, 0x38}},
                {ByteOrderMark.Utf7B, new byte[] {0x2B, 0x2F, 0x76, 0x39}},
                {ByteOrderMark.Utf7C, new byte[] {0x2B, 0x2F, 0x76, 0x2B}},
                {ByteOrderMark.Utf7D, new byte[] {0x2B, 0x2F, 0x76, 0x2F}},
                {ByteOrderMark.Utf7E, new byte[] {0x2B, 0x2F, 0x76, 0x38, 0x2D}},
                {ByteOrderMark.Utf1, new byte[] {0xF7, 0x64, 0x46}},
                {ByteOrderMark.UtfEbcdic, new byte[] {0xDD, 0x73, 0x66, 0x73}},
                {ByteOrderMark.Scsu, new byte[] {0x0E, 0xFE, 0xFF}},
                {ByteOrderMark.Bocu1, new byte[] {0xFB, 0xEE, 0x28}},
                {ByteOrderMark.Gb18030, new byte[] {0x84, 0x31, 0x95, 0x33}}
            };

            internal static SettingsViewModel Settings { get; set; }


            internal static async Task<bool> DocumentViewExitApplication(DocumentViewModel documentViewModel)
            {
                return await ExitApp(documentViewModel) != SaveState.Canceled;
            }

            internal static void Initializer(IDocumentView documentView, QuickPadCommands commands)
            {
                documentView.ViewModel = ServiceProvider.GetService<DocumentViewModel>();

                commands.NewDocumentCommand.Executioner = NewDocument;
                commands.LoadCommand.Executioner = LoadDocument;
                commands.SaveCommand.Executioner = SaveDocument;
                commands.SaveAsCommand.Executioner = SaveAsDocument;
                commands.ExitCommand.Executioner = ExitApplication;

                documentView.ViewModel.Initialize = async viewModel => { await viewModel.InitNewDocument(); };
            }

            private static async Task NewDocument(DocumentViewModel documentViewModel)
            {
                var canceled = false;
                if (documentViewModel.IsDirty)
                {
                    var saved = await AskSaveDocument(documentViewModel, false);
                    canceled = saved == SaveState.Canceled;
                }

                if(!canceled)
                {
                    documentViewModel.Initialize = async viewModel => 
                        { await viewModel.InitNewDocument(); };

                    documentViewModel.Initialize(documentViewModel);

                    Settings.Status("New document initialized.", TimeSpan.FromSeconds(10),
                        SettingsViewModel.Verbosity.Debug);
                }
            }

            private static Task ExitApplication(DocumentViewModel documentViewModel)
            {
                return ExitApp(documentViewModel);
            }

            private static Task<SaveState> ExitApp(DocumentViewModel documentViewModel)
            {
                Settings.ShowSettings = false;

                return documentViewModel.IsDirty
                    ? AskSaveDocument(documentViewModel, true)
                    : Task.FromResult(Close(documentViewModel) == DeferredState.Deferred ? SaveState.Saved : SaveState.Unsaved);
            }

            public enum DeferredState
            {
                NotDeferred, Deferred
            }

            private static DeferredState Close(DocumentViewModel documentViewModel)
            {
                if (documentViewModel.Deferral != null)
                {
                    Settings.NotDeferred = false;
                    documentViewModel.Deferred = true;
                    try
                    {
                        documentViewModel.Deferral.Complete();
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
                    documentViewModel.ExitApplication?.Invoke();
                }

                return DeferredState.NotDeferred;
            }

            public enum SaveState
            {
                Canceled, Saved, Unsaved, DeferredSaved, UnDeferredSaved, DeferredNotSaved, UnDeferredNotSaved
            }

            private static async Task<SaveState> AskSaveDocument(DocumentViewModel documentViewModel, bool isClosing = true)
            {
                if (!documentViewModel.IsDirty && isClosing) 
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

                var dialog = ServiceProvider.GetService<AskToSave>();
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

            private static async Task LoadDocument(DocumentViewModel documentViewModel)
            {
                if (documentViewModel.IsDirty)
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

            public static async Task LoadFile(DocumentViewModel documentViewModel, StorageFile file)
            {
                var provider = new FileDataProvider();
                var bytes = await provider.LoadDataAsync(file);
                var reader = new EncodingReader();
                reader.AddBytes(bytes);

                documentViewModel.CurrentEncoding = Encoding.UTF8;

                try
                {
                    _byteOrderMarks.ToList().ForEach(pair =>
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
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(new EventId(), $"Error loading {file.Name}.", ex);
                    Settings.Status(ex.Message, TimeSpan.FromSeconds(60), SettingsViewModel.Verbosity.Error);
                }

                var text = reader.Read(documentViewModel.CurrentEncoding);

                documentViewModel.File = file;

                documentViewModel.Text = text;

                documentViewModel.ReleaseUpdates();

                Settings.Status($"Loaded {documentViewModel.File.Name}", TimeSpan.FromSeconds(10),
                    SettingsViewModel.Verbosity.Release);

                documentViewModel.IsDirty = false;
            }

            private static Task<SaveState> SaveDocument(DocumentViewModel documentViewModel)
            {
                return SaveDocument(documentViewModel, false);
            }

            private static Task<SaveState> SaveAsDocument(DocumentViewModel documentViewModel)
            {
                return SaveDocument(documentViewModel, true);
            }

            private static async Task<SaveState> SaveDocument(DocumentViewModel documentViewModel, bool saveAs)
            {
                documentViewModel.HoldUpdates();

                if (documentViewModel.File == null || saveAs)
                {
                    var savePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                    };

                    if (documentViewModel.CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        savePicker.FileTypeChoices.Add("Rich Text", new List<string> {".rtf"});
                        savePicker.FileTypeChoices.Add("Plain Text", new List<string> {".txt"});
                        savePicker.FileTypeChoices.Add("Any", new List<string> {"."});
                    }
                    else if (documentViewModel.CurrentFileType.Equals(".txt",
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        savePicker.FileTypeChoices.Add("Plain Text", new List<string> {".txt"});
                        savePicker.FileTypeChoices.Add("Rich Text", new List<string> {".rtf"});
                        savePicker.FileTypeChoices.Add("Any", new List<string> {"."});
                    }
                    else
                    {
                        savePicker.FileTypeChoices.Add("Document",
                            new List<string> {documentViewModel.CurrentFileType});
                        savePicker.FileTypeChoices.Add("Plain Text", new List<string> {".txt"});
                        savePicker.FileTypeChoices.Add("Rich Text", new List<string> {".rtf"});
                        savePicker.FileTypeChoices.Add("Any", new List<string> {"."});
                    }
                    // Dropdown of file types the user can save the file as

                    // Default file name if the user does not type one in or select a file to replace
                    savePicker.SuggestedFileName = documentViewModel.File?.DisplayName ?? DocumentViewModel.Untitled;

                    var file = await savePicker.PickSaveFileAsync();

                    if (file == null) return SaveState.Canceled;
                    documentViewModel.File = file;
                }

                if (Logger.IsEnabled(LogLevel.Debug))
                    Logger.LogDebug($"Saving {documentViewModel.File.DisplayName}:\n{documentViewModel.Text}");

                var writer = new EncodingWriter();
                writer.Write(documentViewModel.Text);

                await new FileDataProvider().SaveDataAsync(documentViewModel.File, writer,
                    documentViewModel.CurrentEncoding);

                documentViewModel.IsDirty = false;

                documentViewModel.ReleaseUpdates();

                Settings.Status($"Saved {documentViewModel.File.Name}", TimeSpan.FromSeconds(10),
                    SettingsViewModel.Verbosity.Release);

                return SaveState.Saved;
            }

            private enum ByteOrderMark
            {
                Utf8,
                Utf16Be,
                Utf16Le,
                Utf32Be,
                Utf32Le,
                Utf7A,
                Utf7B,
                Utf7C,
                Utf7D,
                Utf7E,
                Utf1,
                UtfEbcdic,

                // ReSharper disable once IdentifierTypo
                Scsu,

                // ReSharper disable once IdentifierTypo
                Bocu1,
                Gb18030
            }
        }
    }
}