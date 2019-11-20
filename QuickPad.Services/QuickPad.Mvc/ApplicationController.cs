using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickPad.Data;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvc
{
    public class ApplicationController
    {
        private const string HARDWARE_BACK_BUTTON = "Windows.Phone.UI.Input.HardwareButtons";
        private const string RTF_MARKER = "{\\rtf1";

        private readonly Dictionary<ByteOrderMark, byte[]> _byteOrderMarks = new Dictionary<ByteOrderMark, byte[]>
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

        private readonly List<IView> _views = new List<IView>();

        private SettingsViewModel Settings { get; }

        public ApplicationController(ILogger<ApplicationController> logger, IServiceProvider serviceProvider, SettingsViewModel settings)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            Settings = settings;

            if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Started Application Controller.");
        }

        private ILogger<ApplicationController> Logger { get; }
        public IServiceProvider ServiceProvider { get; }

        public void AddView<TView>(TView view) where TView : IView
        {
            if (_views.Contains(view)) return;

            switch (view)
            {
                case IDocumentView documentView:

                    if (documentView.ViewModel != null) return;

                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug("Added IDocumentView to controller.");

                    _views.Add(view);

                    documentView.Initialize += DocumentInitializer;

                    break;
            }
        }

        private void DocumentInitializer(IDocumentView documentView, QuickPadCommands commands)
        {
            documentView.ViewModel = ServiceProvider.GetService<DocumentViewModel>();

            commands.NewDocumentCommand.Executioner = NewDocument;
            commands.LoadCommand.Executioner = LoadDocument;
            commands.SaveCommand.Executioner = SaveDocument;
            commands.SaveAsCommand.Executioner = SaveAsDocument;
            commands.ExitCommand.Executioner = ExitApplication;

            documentView.ViewModel.Initialize = async viewModel =>
            {
                await viewModel.InitNewDocument();
            };
        }

        private static Task NewDocument(DocumentViewModel documentViewModel)
        {
            documentViewModel.Initialize(documentViewModel);

            return Task.CompletedTask;
        }

        private async Task ExitApplication(DocumentViewModel documentViewModel)
        {
            Settings.ShowSettings = false;

            if (documentViewModel.IsDirty)
            {
                await AskSaveDocument(documentViewModel);
            }
            else
            {
                Close(documentViewModel);
            }
        }
        private void Close(DocumentViewModel documentViewModel)
        {
            if (documentViewModel.Deferral != null)
            {
                Settings.NotDeferred = false;
                documentViewModel.Deferred = true;
                documentViewModel.Deferral.Complete();
            }
            else
            {
                Settings.NotDeferred = true;
                documentViewModel.ExitApplication?.Invoke();
            }
        }

        private async Task AskSaveDocument(DocumentViewModel documentViewModel)
        {
            if (!documentViewModel.IsDirty) return;

            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            var title = resourceLoader.GetString("PendingChangesTitle");
            var content = resourceLoader.GetString("PendingChangesBody");
            var yes = resourceLoader.GetString("YesButton");
            var no = resourceLoader.GetString("NoButton");
            var cancel = resourceLoader.GetString("CancelButton");

            var yesCommand = new UICommand(yes, async _ =>
            {
                await SaveDocument(documentViewModel);
                Close(documentViewModel);
            });


            var noCommand = new UICommand(no, _ => Close(documentViewModel));
            var cancelCommand = new UICommand(cancel, _ => { });

            var dialog = new MessageDialog(content, title)
            {
                Options = MessageDialogOptions.None
                , DefaultCommandIndex = 0
                , CancelCommandIndex = 0
            };

            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(noCommand);
            dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;

            if (ApiInformation.IsTypePresent(HARDWARE_BACK_BUTTON))
            {
                dialog.CancelCommandIndex = uint.MaxValue;
            }
            else
            {
                dialog.Commands.Add(cancelCommand);
                dialog.CancelCommandIndex = (uint) dialog.Commands.Count - 1;
            }

            var command = await dialog.ShowAsync();

            if (command == null)
            {
                cancelCommand.Invoked(cancelCommand);
            }
        }

        private async Task LoadDocument(DocumentViewModel documentViewModel)
        {
            documentViewModel.HoldUpdates();

            var loadPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            loadPicker.FileTypeFilter.Add(".txt");
            loadPicker.FileTypeFilter.Add(".rtf");
            loadPicker.FileTypeFilter.Add("*");

            var file = await loadPicker.PickSingleFileAsync();

            if (file == null) return;

            var provider = new FileDataProvider();
            var bytes = await provider.LoadDataAsync(file);
            var reader = new EncodingReader();
            reader.AddBytes(bytes);

            documentViewModel.CurrentEncoding = Encoding.UTF8;

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

            var text = reader.Read(documentViewModel.CurrentEncoding);

            var isRtf = text.StartsWith(RTF_MARKER, StringComparison.InvariantCultureIgnoreCase);

            documentViewModel.GetOption = isRtf ? TextGetOptions.FormatRtf : TextGetOptions.None;
            documentViewModel.SetOption = isRtf ? TextSetOptions.FormatRtf : TextSetOptions.None;

            documentViewModel.File = file;
            documentViewModel.Text = text;
            documentViewModel.IsDirty = false;

            documentViewModel.ReleaseUpdates();
        }

        private Task SaveDocument(DocumentViewModel documentViewModel)
        {
            return SaveDocument(documentViewModel, false);
        }

        private Task SaveAsDocument(DocumentViewModel documentViewModel)
        {
            return SaveDocument(documentViewModel, true);
        }

        private async Task SaveDocument(DocumentViewModel documentViewModel, bool saveAs)
        {
            documentViewModel.HoldUpdates();

            var writer = new EncodingWriter();
            writer.Write(documentViewModel.Text);

            if (documentViewModel.File == null || saveAs)
            {
                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Plain Text", new List<string> {".txt"});
                savePicker.FileTypeChoices.Add("Rich Text", new List<string> {".rtf"});
                savePicker.FileTypeChoices.Add("Any", new List<string> {"."});

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = documentViewModel.File?.DisplayName ?? "Unnamed";

                var file = await savePicker.PickSaveFileAsync();

                if (file == null) return;
                documentViewModel.File = file;
            }

            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug($"Saving {documentViewModel.File.DisplayName}:\n{documentViewModel.Text}");

            await new FileDataProvider().SaveDataAsync(documentViewModel.File, writer, documentViewModel.CurrentEncoding);

            documentViewModel.IsDirty = false;

            documentViewModel.ReleaseUpdates();
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