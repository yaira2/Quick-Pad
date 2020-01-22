using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Theme;

namespace QuickPad.UI.Helpers
{
    public class RtfDocument : DocumentModel<StorageFile, IRandomAccessStream>
    {
        private bool _queuedChange;

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

        public override void SetDefaults()
        {
            App.TryEnqueue(() =>
            {
                var defaultCharacterFormat = Document.GetDefaultCharacterFormat();

                defaultCharacterFormat.ForegroundColor = Settings.DefaultTextForegroundColor.ToColor();
                defaultCharacterFormat.BackgroundColor =
                    (ServiceProvider.GetService<IVisualThemeSelector>().CurrentItem.SolidBackgroundBrush as SolidColorBrush)?
                    .Color ?? (Color)Application.Current.Resources["SystemColorWindowColor"];
                defaultCharacterFormat.Name = Settings.DefaultRtfFont;
                defaultCharacterFormat.Size = Settings.DefaultFontRtfSize;

                Document.SetDefaultCharacterFormat(defaultCharacterFormat);
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
            get => Document.Selection.CharacterFormat.Name ?? Settings.DefaultRtfFont;
            set
            {
                var name = Document.Selection.CharacterFormat.Name;
                if (Set(ref name, value))
                {
                    App.TryEnqueue(() => Document.Selection.CharacterFormat.Name = name);
                }
            }
        }

        public override float CurrentFontSize
        {
            get => Document.Selection.CharacterFormat.Size;
            set
            {
                var size = Document.Selection.CharacterFormat.Size;
                if (Set(ref size, value))
                {
                    App.TryEnqueue(() => Document.Selection.CharacterFormat.Size = size);
                }
            }
        }

        public override bool SelBold
        {
            get => Document.Selection.FormattedText.CharacterFormat.Bold == FormatEffect.On;
            set
            {
                var bold = Document.Selection.FormattedText.CharacterFormat.Bold;
                if (Set(ref bold, value ? FormatEffect.On : FormatEffect.Off))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Bold = bold;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelItalic
        {
            get => Document.Selection.FormattedText.CharacterFormat.Italic == FormatEffect.On;
            set
            {
                var italic = Document.Selection.FormattedText.CharacterFormat.Italic;
                if (Set(ref italic, value ? FormatEffect.On : FormatEffect.Off))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Italic = italic;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelUnderline
        {
            get => Document.Selection.FormattedText.CharacterFormat.Underline == UnderlineType.Single;
            set
            {
                var underline = Document.Selection.FormattedText.CharacterFormat.Underline;
                if (Set(ref underline, value ? UnderlineType.Single : UnderlineType.None))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Underline = underline;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelStrikethrough
        {
            get => Document.Selection.FormattedText.CharacterFormat.Strikethrough == FormatEffect.On;
            set
            {
                var strikethrough = Document.Selection.FormattedText.CharacterFormat.Strikethrough;
                if (Set(ref strikethrough, value ? FormatEffect.On : FormatEffect.Off))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Strikethrough = strikethrough;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelCenter
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Center;
            set
            {
                var alignment = Document.Selection.FormattedText.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Center : ParagraphAlignment.Undefined))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelRight
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Right;
            set
            {
                var alignment = Document.Selection.FormattedText.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Right : ParagraphAlignment.Undefined))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelLeft
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Left;
            set
            {
                var alignment = Document.Selection.FormattedText.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Left : ParagraphAlignment.Undefined))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelCenter));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelJustify));
                    });
                }
            }
        }

        public override bool SelJustify
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Justify;
            set
            {
                var alignment = Document.Selection.FormattedText.ParagraphFormat.Alignment;
                if (Set(ref alignment, value ? ParagraphAlignment.Justify : ParagraphAlignment.Undefined))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
                        OnPropertyChanged(nameof(SelLeft));
                        OnPropertyChanged(nameof(SelRight));
                        OnPropertyChanged(nameof(SelCenter));
                    });
                }
            }
        }

        public override bool SelBullets
        {
            get => Document.Selection.FormattedText.ParagraphFormat.ListType == MarkerType.Bullet;
            set
            {
                var listType = Document.Selection.FormattedText.ParagraphFormat.ListType;
                if (Set(ref listType, value ? MarkerType.Bullet : MarkerType.None))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.ParagraphFormat.ListType = listType;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelSubscript
        {
            get => Document.Selection.FormattedText.CharacterFormat.Subscript == FormatEffect.On;
            set
            {
                var subscript = Document.Selection.FormattedText.CharacterFormat.Subscript;
                if (Set(ref subscript, value ? FormatEffect.On : FormatEffect.Off))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Subscript = subscript;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override bool SelSuperscript
        {
            get => Document.Selection.FormattedText.CharacterFormat.Superscript == FormatEffect.On;
            set
            {
                var superscript = Document.Selection.FormattedText.CharacterFormat.Superscript;
                if (Set(ref superscript, value ? FormatEffect.On : FormatEffect.Off))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.Superscript = superscript;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override string ForegroundColor
        {
            get => Document.Selection.FormattedText.CharacterFormat.ForegroundColor.ToHex();
            set
            {
                var hex = value.Replace("#", string.Empty);
                var a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);
                var color = Color.FromArgb(a, r, g, b);

                var oldColor = Document.Selection.FormattedText.CharacterFormat.ForegroundColor;
                if (Set(ref oldColor, color))
                {
                    App.TryEnqueue(() =>
                    {
                        Document.Selection.FormattedText.CharacterFormat.ForegroundColor = color;
                        OnPropertyChanged();
                    });
                }
            }
        }

        public override async Task LoadFromStream(QuickPadTextSetOptions options, IRandomAccessStream stream)
        {
            if (stream == null)
            {
                var uri = new System.Uri("ms-appx:///Templates/empty.rtf");
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);

                stream = await file.OpenReadAsync();
            }

            App.TryEnqueue(() => Document.LoadFromStream(options.ToUwp(), stream));
        }

        public override Action<string, bool> Paste => PasteText;

        private void PasteText(string s, bool b)
        {
            if (b)
            {
                App.TryEnqueue(() => Document.Selection.Paste(0));
            }
            else
            {
                App.TryEnqueue(() => Document.Selection.SetText(TextSetOptions.FormatRtf, s));
            }
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
                    OnPropertyChanged(nameof(CurrentFontName));
                    OnPropertyChanged(nameof(CurrentFontSize));
                }
                catch (Exception e)
                {
                    
                }
            }));
        }
    }
}