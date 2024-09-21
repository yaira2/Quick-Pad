using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Theme;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;

namespace QuickPad.UI.Helpers
{
    public class RtfDocument : DocumentModel<StorageFile, IRandomAccessStream>
    {
        private string _currentFontName;
        private float _currentFontSize;

        public RtfDocument(
            ITextDocument document
            , IServiceProvider serviceProvider
            , ILogger<RtfDocument> logger
            , DocumentViewModel<StorageFile, IRandomAccessStream> viewModel
            , WindowsSettingsViewModel settings
            , IApplication<StorageFile, IRandomAccessStream> app)
            : base(logger, viewModel, settings, app)
        {
            Document = document;
            ServiceProvider = serviceProvider;

            viewModel.RedoRequested += model => Redo();
            viewModel.UndoRequested += model => Undo();
        }

        public override void SetDefaults(Action continueWith)
        {
            App.TryEnqueue(() =>
            {
                var defaultCharacterFormat = Document.GetDefaultCharacterFormat();

                defaultCharacterFormat.ForegroundColor = VisualThemeSelector.Current.CurrentItem.DefaultTextForegroundColor;
                defaultCharacterFormat.Name = Settings.DefaultRtfFont;
                defaultCharacterFormat.Size = Settings.DefaultFontRtfSize;

                Document.SetDefaultCharacterFormat(defaultCharacterFormat);

                CurrentWordWrap = Settings.RtfWordWrap;
                CurrentFontName = Settings.DefaultRtfFont;
                CurrentFontSize = Settings.DefaultFontRtfSize;

                continueWith?.Invoke();
            });
        }

        public override void Initialize()
        {
            Task.Run(async () =>
            {
                await LoadFromStream(QuickPadTextSetOptions.FormatRtf, null);
                await Task.Delay(1000);
                ForegroundColor = Settings.DefaultTextForegroundColor;
            });
        }

        private void ViewModelOnSetSelection(int arg1, int arg2, bool arg3)
        {
            NotifyOnSelectionChange();
        }

        public ITextDocument Document { get; }
        public IServiceProvider ServiceProvider { get; }

        public override bool CanCopy => Document.CanCopy();
        public override bool CanPaste => Document.CanPaste();
        public override bool CanRedo => Document.CanRedo();
        public override bool CanUndo => Document.CanUndo();

        public override void Redo()
        {
            App.TryEnqueue(Document.Redo);
        }

        public override void Undo()
        {
            App.TryEnqueue(Document.Undo);
        }

        public override void SetText(QuickPadTextSetOptions options, string value)
        {
            App.TryEnqueue(() => Document.SetText(options.ToUwp(), value));
            OnPropertyChanged(nameof(Text));
        }

        public override async Task<string> GetTextAsync(QuickPadTextGetOptions options) =>
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => GetText(options));

        // Only call on main thread!
        public override string GetText(QuickPadTextGetOptions options)
        {
            Document.GetText(options.ToUwp(), out var value);

            return value;
        }

        public override string CurrentFontName
        {
            get => _currentFontName ??= Settings.DefaultRtfFont;
            set
            {
                if (Set(ref _currentFontName, value, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Name = _currentFontName;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override float CurrentFontSize
        {
            get => (_currentFontSize.Equals(default) ? Settings.DefaultFontRtfSize : _currentFontSize);
            set
            {
                if (Set(ref _currentFontSize, value, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Size = _currentFontSize;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelBold
        {
            get => Document.Selection.CharacterFormat.Bold == FormatEffect.On;
            set
            {
                var bold = Document.Selection.CharacterFormat.Bold;
                if (Set(ref bold, value ? FormatEffect.On : FormatEffect.Off, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Bold = bold;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Bold = bold;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged(nameof(SelBold));
                    });
                }
            }
        }

        public override bool SelItalic
        {
            get => Document.Selection.CharacterFormat.Italic == FormatEffect.On;
            set
            {
                var italic = Document.Selection.CharacterFormat.Italic;
                if (Set(ref italic, value ? FormatEffect.On : FormatEffect.Off, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Italic = italic;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Italic = italic;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelUnderline
        {
            get => Document.Selection.CharacterFormat.Underline == UnderlineType.Single;
            set
            {
                var underline = Document.Selection.CharacterFormat.Underline;
                if (Set(ref underline, value ? UnderlineType.Single : UnderlineType.None, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Underline = underline;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Underline = underline;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelStrikethrough
        {
            get => Document.Selection.CharacterFormat.Strikethrough == FormatEffect.On;
            set
            {
                var strikethrough = Document.Selection.CharacterFormat.Strikethrough;
                if (Set(ref strikethrough, value ? FormatEffect.On : FormatEffect.Off, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Strikethrough = strikethrough;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Strikethrough = strikethrough;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelCenter
        {
            get => Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Center;
            set
            {
                var alignment = Document.Selection.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Center : ParagraphAlignment.Undefined, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelRight
        {
            get => Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Right;
            set
            {
                var alignment = Document.Selection.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Right : ParagraphAlignment.Undefined, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelLeft
        {
            get => Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Left;
            set
            {
                var alignment = Document.Selection.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Left : ParagraphAlignment.Undefined, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelJustify
        {
            get => Document.Selection.ParagraphFormat.Alignment == ParagraphAlignment.Justify;
            set
            {
                var alignment = Document.Selection.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Justify : ParagraphAlignment.Undefined, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelBullets
        {
            get => Document.Selection.ParagraphFormat.ListType == MarkerType.Bullet;
            set
            {
                var listType = Document.Selection.ParagraphFormat.ListType;
                if (Set(ref listType, value ? MarkerType.Bullet : MarkerType.None, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.ParagraphFormat.ListType = listType;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelSubscript
        {
            get => Document.Selection.CharacterFormat.Subscript == FormatEffect.On;
            set
            {
                var subscript = Document.Selection.CharacterFormat.Subscript;
                if (Set(ref subscript, value ? FormatEffect.On : FormatEffect.Off, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Subscript = subscript;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Subscript = subscript;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelSuperscript
        {
            get => Document.Selection.CharacterFormat.Superscript == FormatEffect.On;
            set
            {
                var superscript = Document.Selection.CharacterFormat.Superscript;
                if (Set(ref superscript, value ? FormatEffect.On : FormatEffect.Off, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.Superscript = superscript;

                        if (Document.Selection.Length == 0)
                        {
                            var defaults = Document.GetDefaultCharacterFormat();
                            defaults.Superscript = superscript;
                            Document.SetDefaultCharacterFormat(defaults);
                        }
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override string ForegroundColor
        {
            get => Document.Selection.CharacterFormat.ForegroundColor.ToHex();
            set
            {
                var hex = value.Replace("#", string.Empty);
                var a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);
                var color = Color.FromArgb(a, r, g, b);

                var oldColor = Document.Selection.CharacterFormat.ForegroundColor;
                if (Set(ref oldColor, color, notify: false))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.CharacterFormat.ForegroundColor = color;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override Task<bool> LoadFromStream(QuickPadTextSetOptions options, IRandomAccessStream stream)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (stream == null)
                {
                    var uri = new Uri("ms-appx:///Templates/empty.rtf");

                    var fileTask = StorageFile.GetFileFromApplicationUriAsync(uri);

                    var file = fileTask.GetAwaiter().GetResult();

                    var task = file.OpenReadAsync();

                    stream = task.GetAwaiter().GetResult();
                }

                App.TryEnqueue(() =>
                {
                    try
                    {
                        Document.LoadFromStream(options.ToUwp(), stream);
                        IsDirty = false;
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "LoadFromStream caught exception.");

                dynamic error = e.Data["__RestrictedErrorObject"];
            }

            return tcs.Task;
        }

        public override Action<string, bool> Paste => PasteText;

        private void PasteText(string s, bool b)
        {
            var mre = new ManualResetEvent(false);

            if (b)
            {
                App.TryEnqueue(() =>
                {
                    Document.Selection.Paste(0);
                    mre.Set();
                });
            }
            else
            {
                App.TryEnqueue(() =>
                {
                    Document.Selection.SetText(TextSetOptions.FormatRtf, s);
                    mre.Set();
                });
            }

            mre.WaitOne();
        }

        public override void BeginUndoGroup()
        {
            Document.BeginUndoGroup();
        }

        public override void EndUndoGroup()
        {
            Document.EndUndoGroup();
        }

        public override void SetSelectedText(string text)
        {
            App.TryEnqueue(() =>
            {
                Document.Selection.Text = text;

                OnPropertyChanged(nameof(Text));
            });
        }

        public override (int start, int length) GetSelectionBounds()
        {
            return (Document.Selection.StartPosition, Document.Selection.Length);
        }

        public override (int start, int length) SetSelectionBound(int start, int length)
        {
            var s = Math.Min(start, start + length);
            var e = Math.Max(start, start + length);

            Document.Selection.StartPosition = s;
            Document.Selection.EndPosition = e;

            NotifyOnSelectionChange();

            return GetSelectionBounds();
        }

        public override void NotifyOnSelectionChange()
        {
            App.DoWhenIdle((Action)(() =>
            {
                try
                {
                    OnPropertyChanged(nameof(SelSuperscript));
                    OnPropertyChanged(nameof(SelBold));
                    OnPropertyChanged(nameof(SelBullets));
                    OnPropertyChanged(nameof(SelCenter));
                    OnPropertyChanged(nameof(SelItalic));
                    OnPropertyChanged(nameof(SelJustify));
                    OnPropertyChanged(nameof(SelLeft));
                    OnPropertyChanged(nameof(SelRight));
                    OnPropertyChanged(nameof(SelStrikethrough));
                    OnPropertyChanged(nameof(SelUnderline));
                }
                catch (Exception)
                {
                }
            }));
        }
    }
}