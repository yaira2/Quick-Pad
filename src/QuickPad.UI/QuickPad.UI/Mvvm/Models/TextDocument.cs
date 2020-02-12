using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Theme;

namespace QuickPad.UI.Helpers
{
    public class TextDocument : DocumentModel<StorageFile, IRandomAccessStream>
    {
        private string _currentFontName;
        private float? _currentFontSize;

        public TextDocument(
            TextBox document
            , ILogger<TextDocument> logger
            , DocumentViewModel<StorageFile, IRandomAccessStream> viewModel
            , WindowsSettingsViewModel settings
            , IApplication<StorageFile, IRandomAccessStream> app)
            : base(logger, viewModel, settings, app)
        {
            Document = document;
        }

        public TextBox Document { get; }

        public override bool CanCopy => Document.SelectionLength > 0;
        public override bool CanPaste => true;
        public override bool CanRedo => Document.CanRedo;
        public override bool CanUndo => Document.CanUndo;
        
        public override void SetDefaults(Action continueWith)
        {
            App.TryEnqueue(() =>
            {
                CurrentWordWrap = Settings.WordWrap;
                CurrentFontName = Settings.DefaultFont;
                CurrentFontSize = Settings.DefaultFontSize;

                continueWith?.Invoke();
            });
        }

        public override void Initialize()
        {
            Text = "";
        }

        public override void Redo()
        {
            App.TryEnqueue(() => Document.Redo());
        }

        public override void Undo()
        {
            App.TryEnqueue(() => Document.Undo());
        }

        public override string CurrentFontName
        {
            get => _currentFontName ??= Settings.DefaultFont;
            set => App.TryEnqueue(() => Document.FontFamily = new FontFamily(_currentFontName = value));
        }

        public override float CurrentFontSize
        {
            get => _currentFontSize ??= Settings.DefaultFontSize;
            set => App.TryEnqueue(() => Document.FontSize = (double)(_currentFontSize = value));
        }

        public override string ForegroundColor
        {
            get => (Document.Foreground as SolidColorBrush)?.Color.ToHex();
            set
            {
                var hex = value.Replace("#", string.Empty);
                var a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);
                var color = Color.FromArgb(a, r, g, b);

                App.TryEnqueue(() => Document.Foreground = new SolidColorBrush(color));
            }
        }

        public override Task<bool> LoadFromStream(QuickPadTextSetOptions options, IRandomAccessStream stream)
        {
            var tcs = new TaskCompletionSource<bool>();
            var memoryStream = new InMemoryRandomAccessStream();
            var bytes = Encoding.UTF8.GetBytes("\r");
            var buffer = bytes.AsBuffer();
            var task = memoryStream.WriteAsync(buffer);
            
            task.GetResults();

            memoryStream.Seek(0);

            App.TryEnqueue(() =>
            {
                try
                {
                    Text = stream.ReadTextAsync(ViewModel.CurrentEncoding).Result;
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public override Action<string, bool> Paste => (s, b) => App.TryEnqueue(() => Document.SelectedText = s);

        public override void BeginUndoGroup()
        {
        }

        public override void EndUndoGroup()
        {
        }

        public override void SetSelectedText(string text)
        {
            App.TryEnqueue(() => Document.SelectedText = text);
        }

        public override (int start, int length) GetSelectionBounds()
        {
            return (Document.SelectionStart, Document.SelectionLength);
        }

        public override (int start, int length) SetSelectionBound(int start, int length)
        {
            Document.SelectionStart = start;
            Document.SelectionLength = length;

            return GetSelectionBounds();
        }

        public override void NotifyOnSelectionChange()
        {
            // Text Document doesn't need to update UI based on selection.
        }

        public override void SetText(QuickPadTextSetOptions options, string value)
        {
            App.TryEnqueue(() =>
                Document.Text = value);
        }

        public override async Task<string> GetTextAsync(QuickPadTextGetOptions options) =>
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => Document.Text);

        // Only call on main thread!
        public override string GetText(QuickPadTextGetOptions options)
        {
            return Document.Text;
        }

    }
}