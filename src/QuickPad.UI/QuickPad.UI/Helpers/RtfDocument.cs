using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Helpers
{
    public class RtfDocument : DocumentModel<StorageFile, IRandomAccessStream>
    {
        public RtfDocument(
            ITextDocument document
            , ILogger<RtfDocument> logger
            , DocumentViewModel<StorageFile, IRandomAccessStream> viewModel
            , WindowsSettingsViewModel settings
            , IApplication<StorageFile, IRandomAccessStream> app) 
            : base(logger, viewModel, settings, app)
        {
            Document = document;

            viewModel.RedoRequested += model => Redo();
            viewModel.UndoRequested += model => Undo();
        }

        private void ViewModelOnSetSelection(int arg1, int arg2, bool arg3)
        {
            
        }

        public ITextDocument Document { get; }

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
        }

        public override void GetText(QuickPadTextGetOptions options, out string value)
        {
            Document.GetText(options.ToUwp(), out value);
        }

        public override bool SelBold 
        { 
            get => Document.Selection.FormattedText.CharacterFormat.Bold == FormatEffect.On; 
            set => Document.Selection.FormattedText.CharacterFormat.Bold = value ? FormatEffect.On : FormatEffect.Off;
        }

        public override bool SelItalic
        {
            get => Document.Selection.FormattedText.CharacterFormat.Italic == FormatEffect.On;
            set => Document.Selection.FormattedText.CharacterFormat.Italic = value ? FormatEffect.On : FormatEffect.Off;
        }

        public override bool SelUnderline
        {
            get => Document.Selection.FormattedText.CharacterFormat.Underline == UnderlineType.Single;
            set => Document.Selection.FormattedText.CharacterFormat.Underline = value ? UnderlineType.Single : UnderlineType.None;
        }

        public override bool SelStrikethrough
        {
            get => Document.Selection.FormattedText.CharacterFormat.Strikethrough == FormatEffect.On;
            set => Document.Selection.FormattedText.CharacterFormat.Strikethrough = value ? FormatEffect.On : FormatEffect.Off;
        }

        public override bool SelCenter
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Center;
            set => Document.Selection.FormattedText.ParagraphFormat.Alignment = value ? ParagraphAlignment.Center : ParagraphAlignment.Undefined;
        }

        public override bool SelRight
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Right;
            set => Document.Selection.FormattedText.ParagraphFormat.Alignment = value ? ParagraphAlignment.Right : ParagraphAlignment.Undefined;
        }

        public override bool SelLeft
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Left;
            set => Document.Selection.FormattedText.ParagraphFormat.Alignment = value ? ParagraphAlignment.Left : ParagraphAlignment.Undefined;
        }

        public override bool SelJustify
        {
            get => Document.Selection.FormattedText.ParagraphFormat.Alignment == ParagraphAlignment.Justify;
            set => Document.Selection.FormattedText.ParagraphFormat.Alignment = value ? ParagraphAlignment.Justify : ParagraphAlignment.Undefined;
        }

        public override bool SelBullets
        {
            get => Document.Selection.FormattedText.ParagraphFormat.ListType == MarkerType.Bullet;
            set => Document.Selection.FormattedText.ParagraphFormat.ListType = value ? MarkerType.Bullet : MarkerType.Undefined;
        }

        public override bool SelSubscript
        {
            get => Document.Selection.FormattedText.CharacterFormat.Subscript == FormatEffect.On;
            set => Document.Selection.FormattedText.CharacterFormat.Subscript = value ? FormatEffect.On : FormatEffect.Off;
        }

        public override bool SelSuperscript
        {
            get => Document.Selection.FormattedText.CharacterFormat.Superscript == FormatEffect.On;
            set => Document.Selection.FormattedText.CharacterFormat.Superscript = value ? FormatEffect.On : FormatEffect.Off;
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

                Document.Selection.FormattedText.CharacterFormat.ForegroundColor = color;
            }
        }

        public override async Task LoadFromStream(QuickPadTextSetOptions options, IRandomAccessStream stream)
        {
            var memoryStream = new InMemoryRandomAccessStream();
            var bytes = Encoding.UTF8.GetBytes("\r");
            var buffer = bytes.AsBuffer();
            await memoryStream.WriteAsync(buffer);

            memoryStream.Seek(0);

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

            return GetSelectionBounds();
        }
    }
}