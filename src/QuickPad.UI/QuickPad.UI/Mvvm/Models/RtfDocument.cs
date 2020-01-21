using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
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
            Document.Redo();
        }

        public override void Undo()
        {
            Document.Undo();
        }

        public override void SetText(QuickPadTextSetOptions options, string value)
        {
            Document.SetText(options.ToUwp(), value);
            OnPropertyChanged(nameof(Text));
        }

        public override void GetText(QuickPadTextGetOptions options, out string value)
        {
            Document.GetText(options.ToUwp(), out value);
        }

        public override string CurrentFontName
        {
            get => Document.Selection.CharacterFormat.Name;
            set
            {
                var name = Document.Selection.CharacterFormat.Name;
                if (Set(ref name, value))
                {
                    Document.Selection.CharacterFormat.Name = name;
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
                    Document.Selection.CharacterFormat.Size = size;
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
                    Document.Selection.FormattedText.CharacterFormat.Bold = bold;
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
                    Document.Selection.FormattedText.CharacterFormat.Italic = italic;
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
                    Document.Selection.FormattedText.CharacterFormat.Underline = underline;
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
                    Document.Selection.FormattedText.CharacterFormat.Strikethrough = strikethrough;
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
                    Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
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
                    Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
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
                    Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
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
                    Document.Selection.FormattedText.ParagraphFormat.Alignment = alignment;
                }
            }
        }

        public override bool SelBullets
        {
            get => Document.Selection.FormattedText.ParagraphFormat.ListType == MarkerType.Bullet;
            set
            {
                var listType = Document.Selection.FormattedText.ParagraphFormat.ListType;
                if (Set(ref listType, value ? MarkerType.Bullet : MarkerType.Undefined))
                {
                    Document.Selection.FormattedText.ParagraphFormat.ListType = listType;
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
                    Document.Selection.FormattedText.CharacterFormat.Subscript = subscript;
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
                    Document.Selection.FormattedText.CharacterFormat.Superscript = superscript;
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
                    Document.Selection.FormattedText.CharacterFormat.ForegroundColor = color;
                }
            }
        }

        public override async Task LoadFromStream(QuickPadTextSetOptions options, IRandomAccessStream stream)
        {
            if(stream == null)
            {
                var uri = new System.Uri("ms-appx:///Templates/empty.rtf");
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);

                stream = await file.OpenReadAsync();
            }

            Document.LoadFromStream(options.ToUwp(), stream);
        }

        public override Action<string, bool> Paste => PasteText;

        private void PasteText(string s, bool b)
        {
            if (b)
            {
                Document.Selection.Paste(0);
            }
            else
            {
                Document.Selection.SetText(TextSetOptions.FormatRtf, s);
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
            Document.Selection.Text = text;
            OnPropertyChanged(nameof(Text));
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
    }
}