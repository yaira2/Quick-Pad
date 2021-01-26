using Microsoft.Extensions.Logging;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickPad.Mvc
{
    public abstract class DocumentManager<TStorageFile, TStream, TDocumentManager>
        where TDocumentManager : DocumentManager<TStorageFile, TStream, TDocumentManager>
        where TStream : class
    {
        private const string HARDWARE_BACK_BUTTON = "Windows.Phone.UI.Input.HardwareButtons";
        private const string RTF_MARKER = "{\\rtf1";

        protected internal ILogger<ApplicationController<TStorageFile, TStream, TDocumentManager>> Logger { get; set; }
        protected internal IServiceProvider ServiceProvider { get; set; }
        protected internal IApplication<TStorageFile, TStream> App { get; protected set; }

        public IEnumerable<IDocumentView<TStorageFile, TStream>> Views { get; set; }

        protected readonly Dictionary<ByteOrderMark, byte[]> ByteOrderMarks =
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

        protected internal SettingsViewModel<TStorageFile, TStream> Settings { get; set; }

        protected internal abstract void DocumentViewOnGainedFocus();

        protected internal abstract Task<bool> DocumentViewExitApplication(
            DocumentViewModel<TStorageFile, TStream> documentViewModel);

        public abstract void Initializer(IDocumentView<TStorageFile, TStream> documentView,
            IQuickPadCommands<TStorageFile, TStream> commands
            , IApplication<TStorageFile, TStream> app);

        protected abstract Task<SaveState> SaveAsDocument(DocumentViewModel<TStorageFile, TStream> arg);

        protected abstract Task<SaveState> SaveDocument(DocumentViewModel<TStorageFile, TStream> arg);

        public abstract Task NewDocument(DocumentViewModel<TStorageFile, TStream> documentViewModel);

        protected abstract Task ExitApplication(DocumentViewModel<TStorageFile, TStream> documentViewModel);

        protected virtual Task<SaveState> ExitApp(DocumentViewModel<TStorageFile, TStream> documentViewModel)
        {
            Settings.ShowSettings = false;

            return documentViewModel.Document.IsDirty
                ? AskSaveDocument(documentViewModel, true)
                : Task.FromResult(Close(documentViewModel) == DeferredState.Deferred
                    ? SaveState.Saved
                    : SaveState.Unsaved);
        }

        protected abstract DeferredState Close(DocumentViewModel<TStorageFile, TStream> documentViewModel);

        protected abstract Task<SaveState> AskSaveDocument(DocumentViewModel<TStorageFile, TStream> documentViewModel,
            bool isClosing = true);

        protected abstract Task LoadDocument(DocumentViewModel<TStorageFile, TStream> documentViewModel);

        public abstract Task LoadFile(DocumentViewModel<TStorageFile, TStream> viewModel, TStorageFile file);

        protected internal abstract Task<SaveState> SaveDocument(DocumentViewModel<TStorageFile, TStream> documentViewModel, bool saveAs);

        protected internal abstract Task<SaveState> SaveSilently(DocumentViewModel<TStorageFile, TStream> documentViewModel, string cacheFilename);

        public enum DeferredState
        {
            NotDeferred,
            Deferred
        }

        public enum SaveState
        {
            Canceled,
            Saved,
            Unsaved,
            DeferredSaved,
            UnDeferredSaved,
            DeferredNotSaved,
            UnDeferredNotSaved
        }

        protected enum ByteOrderMark
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

        public abstract void SaveDocument(IDocumentView<TStorageFile, TStream> obj);

        public abstract void SaveDocumentToCache(IDocumentView<TStorageFile, TStream> obj, string cacheFilename);
    }
}