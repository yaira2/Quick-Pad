using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Models
{
    public abstract class DocumentModel<TStorageFile, TStream> : ViewModel<TStorageFile, TStream>, IDocumentModel
        where TStream : class
    {
        protected DocumentModel(
            ILogger<DocumentModel<TStorageFile, TStream>> logger
            , DocumentViewModel<TStorageFile, TStream> viewModel
            , SettingsViewModel<TStorageFile, TStream> settings
            , IApplication<TStorageFile, TStream> app) : base(logger, app)
        {
            ViewModel = viewModel;
            Settings = settings;
        }

        public DocumentViewModel<TStorageFile, TStream> ViewModel { get; }
        public SettingsViewModel<TStorageFile, TStream> Settings { get; }

        public void CalculateDirty(string text = null)
        {
            var toCompare = _marked?.TrimEnd('\r') ?? string.Empty;
            text = text?.TrimEnd('\r');

            if (text?.Length == toCompare.Length)
            {
                for (var i = 0; i < Math.Min(text?.Length ?? 0, toCompare.Length); ++i)
                {
                    if (text?[i] == toCompare[i]) continue;

                    NotifyListeners();

                    return;
                }

                if (IsDirty)
                {
                    IsDirty = false;
                }
            }
            else
            {
                NotifyListeners();

                return;
            }

            void NotifyListeners()
            {
                IsDirty = true;

                OnPropertyChanged(nameof(DocumentViewModel<TStorageFile, TStream>.IsDirtyMarker));
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Text));
            }
        }

        public abstract bool CanCopy { get; }

        public abstract bool CanPaste { get; }

        public abstract bool CanRedo { get; }

        public abstract bool CanUndo { get; }

        public abstract Task<string> GetTextAsync(QuickPadTextGetOptions options);

        public abstract string GetText(QuickPadTextGetOptions options);

        public abstract void Redo();

        public abstract void SetText(QuickPadTextSetOptions options, string value);

        public abstract void Undo();

        public QuickPadTextGetOptions GetOptions { get; set; }
        public QuickPadTextSetOptions SetOptions { get; set; }

        public string Text
        {
            get => GetText(GetOptions);

            set => SetText(SetOptions, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                App.TryEnqueue(() =>
                {
                    Set(ref _isDirty, value);

                    if (!_isDirty)
                    {
                        _marked = GetText(GetOptions);
                    }
                });
            }
        }
        public bool IsUnsavedCache { get; set; } = true;

        public string CacheFilename { get; set; }

        public string Title => ViewModel.Title;

        public abstract string CurrentFontName { get; set; }

        public abstract float CurrentFontSize { get; set; }

        public bool CurrentWordWrap
        {
            get => ViewModel.IsRtf ? Settings.RtfWordWrap : Settings.WordWrap;
            set
            {
                if (ViewModel.IsRtf)
                {
                    Settings.RtfWordWrap = value;
                }
                else
                {
                    Settings.WordWrap = value;
                }
            }
        }

        private bool _selBold = false;

        public virtual bool SelBold
        {
            get => _selBold;
            set => Set(ref _selBold, value);
        }

        private bool _selItalic = false;

        public virtual bool SelItalic
        {
            get => _selItalic;
            set => Set(ref _selItalic, value);
        }

        private bool _selUnderline = false;

        public virtual bool SelUnderline
        {
            get => _selUnderline;
            set => Set(ref _selUnderline, value);
        }

        private bool _selStrikethrough = false;

        public virtual bool SelStrikethrough
        {
            get => _selStrikethrough;
            set => Set(ref _selStrikethrough, value);
        }

        private bool _selCenter = false;

        public virtual bool SelCenter
        {
            get => _selCenter;
            set => Set(ref _selCenter, value);
        }

        private bool _selRight = false;

        public virtual bool SelRight
        {
            get => _selRight;
            set => Set(ref _selRight, value);
        }

        private bool _selLeft = false;

        public virtual bool SelLeft
        {
            get => _selLeft;
            set => Set(ref _selLeft, value);
        }

        private bool _selJustify = false;

        public virtual bool SelJustify
        {
            get => _selJustify;
            set => Set(ref _selJustify, value);
        }

        private bool _selBullets = false;

        public virtual bool SelBullets
        {
            get => _selBullets;
            set => Set(ref _selBullets, value);
        }

        private bool _selSubscript = false;

        public virtual bool SelSubscript
        {
            get => _selSubscript;
            set => Set(ref _selSubscript, value);
        }

        private bool _selSuperscript = false;
        private bool _isDirty;
        private string _marked;

        public virtual bool SelSuperscript
        {
            get => _selSuperscript;
            set => Set(ref _selSuperscript, value);
        }

        public List<int> LineIndices { get; } = new List<int>();

        public abstract string ForegroundColor { get; set; }

        public abstract Task<bool> LoadFromStream(QuickPadTextSetOptions options, TStream stream = null);

        public abstract Action<string, bool> Paste { get; }

        public abstract void BeginUndoGroup();

        public abstract void EndUndoGroup();

        public void Reindex()
        {
            LineIndices.Clear();

            var index = -1;
            var text = ViewModel.Text.Replace(Environment.NewLine, "\r").TrimEnd('\r');
            while ((index = text.IndexOf('\r', index + 1)) > -1)
            {
                index++;
                LineIndices.Add(index);

                if (index + 1 >= text.Length) break;
            }

            OnPropertyChanged(nameof(IsDirty));
        }

        public abstract void SetSelectedText(string text);

        public abstract (int start, int length) GetSelectionBounds();

        public abstract (int start, int length) SetSelectionBound(int start, int length);

        public abstract void NotifyOnSelectionChange();

        public abstract void SetDefaults(Action continueWith);

        public abstract void Initialize();
    }
}