using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Models
{
    public abstract class DocumentModel<TStorageFile, TStream> : ViewModel<TStorageFile, TStream>, IDocumentModel
        where TStream : class
    {
        public DocumentViewModel<TStorageFile, TStream> ViewModel { get; }
        public SettingsViewModel<TStorageFile, TStream> Settings { get; }
        private string _originalHash;
        private string _currentHash;
        private readonly HMAC _md5;
        private string _currentFontName;
        private double _currentFontSize = 14;

        protected DocumentModel(
            ILogger<DocumentModel<TStorageFile, TStream>> logger
            , DocumentViewModel<TStorageFile, TStream> viewModel
            , SettingsViewModel<TStorageFile, TStream> settings
            , IApplication<TStorageFile, TStream> app) : base(logger, app)
        {
            ViewModel = viewModel;
            Settings = settings;
            _md5 = HMAC.Create("HMACMD5");
            _md5.Key = Encoding.ASCII.GetBytes("12345");
        }

        public void CalculateHash(string text = null)
        {
            text ??= Text;
            var hash = _md5.ComputeHash(Encoding.ASCII.GetBytes(text ?? string.Empty));

            var newHash = Encoding.ASCII.GetString(hash);

            if (newHash == _currentHash) return;

            _currentHash = newHash;

            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(MediaTypeNames.Text));
        }

        public abstract bool CanCopy { get; }

        public abstract bool CanPaste { get; }

        public abstract bool CanRedo { get; }

        public abstract bool CanUndo { get; }

        public abstract void GetText(QuickPadTextGetOptions options, out string value);

        public abstract void Redo();

        public abstract void SetText(QuickPadTextSetOptions options, string value);

        public abstract void Undo();

        public QuickPadTextGetOptions GetOptions { get; set; }
        public QuickPadTextSetOptions SetOptions { get; set; }

        public string Text
        {
            get
            {
                GetText(GetOptions, out var result);

                return result;
            }

            set => SetText(SetOptions, value);
        }

        public bool IsDirty
        {
            get => (_originalHash != _currentHash);
            set
            {
                if (!value)
                {
                    CalculateHash();
                    _originalHash = _currentHash;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => ViewModel.Title;

        public string CurrentFontName
        {
            get => _currentFontName ?? Settings.DefaultFont;
            set => Set(ref _currentFontName, value);
        }

        public double CurrentFontSize
        {
            get => _currentFontSize;
            set => Set(ref _currentFontSize, value);
        }

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
        public virtual bool SelSuperscript
        {
            get => _selSuperscript;
            set => Set(ref _selSuperscript, value);
        }

        public List<int> LineIndices { get; } = new List<int>();
        
        public abstract string ForegroundColor { get; set; }

        public abstract Task LoadFromStream(QuickPadTextSetOptions options, TStream stream = null);

        public abstract Action<string, bool> Paste { get; }
        
        public abstract void BeginUndoGroup();
        public abstract void EndUndoGroup();

        public void Reindex()
        {
            LineIndices.Clear();

            var index = -1;
            var text = ViewModel.Text.Replace(Environment.NewLine, "\r");
            while ((index = text.IndexOf('\r', index + 1)) > -1)
            {
                index++;
                LineIndices.Add(index);

                if (index + 1 >= text.Length) break;
            }
        }

        public abstract void SetSelectedText(string text);

        public abstract (int start, int length) GetSelectionBounds();
        public abstract (int start, int length) SetSelectionBound(int start, int length);
    }
}