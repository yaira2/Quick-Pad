using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.Commands;
//using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvvm.ViewModels
{
    public class DocumentViewModel<TStorageFile, TStream> : ViewModel<TStorageFile, TStream>
        where TStream : class
    {
        public const string RtfExtension = ".rtf";
        private DocumentModel<TStorageFile, TStream> _document;

        private Encoding _currentEncoding;

        private StorageFileWrapper<TStorageFile> _file;
        private string _currentFileType;
        private string _currentFileDisplayType;
        private int _currentColumn;
        private int _currentLine;
        private int _lineToGoTo;

        private readonly Timer _timer;
        private IFindAndReplaceView<TStorageFile, TStream> _findAndReplaceViewModel;
        private bool _showFind;
        private bool _showReplace;

        public DocumentViewModel(
            ILogger<DocumentViewModel<TStorageFile, TStream>> logger
            , IFindAndReplaceView<TStorageFile, TStream> findAndReplaceViewModel
            , IServiceProvider serviceProvider
            , SettingsViewModel<TStorageFile, TStream> settings
            , IDocumentViewModelStrings strings
            , IApplication<TStorageFile, TStream> app
            , IQuickPadCommands<TStorageFile, TStream> commands) 
            : base(logger, app)
        {
            ServiceProvider = serviceProvider;
            FindAndReplaceViewModel = findAndReplaceViewModel;

            Settings = settings;
            Commands = commands;

            RichTextDescription = strings.RichTextDescription;
            TextDescription = strings.TextDescription;
            Untitled = strings.Untitled;

            _timer = new Timer(AutoSaveTimer);
        }

        public static string Untitled { get; private set; }

        public IFindAndReplaceView<TStorageFile, TStream> FindAndReplaceViewModel
        {
            get => _findAndReplaceViewModel;
            set => Set(ref _findAndReplaceViewModel, value);
        }

        private SettingsViewModel<TStorageFile, TStream> Settings { get; }
        public IQuickPadCommands<TStorageFile, TStream> Commands { get; }

        private IServiceProvider ServiceProvider { get; }

        //public SettingsViewModel Settings => ServiceProvider.GetService<SettingsViewModel<TStorageFile, TStream>>();

        public void InvokeFocusTextBox() => RequestFocusTextBox?.Invoke();

        public event Action RequestFocusTextBox;

        public DocumentModel<TStorageFile, TStream> Document
        {
            get => _document;
            set => Set(ref _document, value);
        }

        private const QuickPadTextGetOptions DEFAULT_TEXT_GET_OPTIONS_TXT = QuickPadTextGetOptions.None | QuickPadTextGetOptions.AdjustCrlf | QuickPadTextGetOptions.NoHidden | QuickPadTextGetOptions.UseCrlf;
        private const QuickPadTextSetOptions DEFAULT_TEXT_SET_OPTIONS_TXT = QuickPadTextSetOptions.None | QuickPadTextSetOptions.Unhide;

        private const QuickPadTextGetOptions DEFAULT_TEXT_GET_OPTIONS_RTF =
            QuickPadTextGetOptions.FormatRtf | QuickPadTextGetOptions.IncludeNumbering | QuickPadTextGetOptions.NoHidden;
        private const QuickPadTextSetOptions DEFAULT_TEXT_SET_OPTIONS_RTF = QuickPadTextSetOptions.FormatRtf | QuickPadTextSetOptions.Unhide;


        public QuickPadTextGetOptions GetOption { get; private set; } = DEFAULT_TEXT_GET_OPTIONS_TXT;
        public QuickPadTextSetOptions SetOption { get; private set; } = DEFAULT_TEXT_SET_OPTIONS_TXT;

        public string CurrentFileType
        {
            get => _currentFileType ?? Settings.DefaultFileType;
            set
            {
                if (!Set(ref _currentFileType, value)) return;

                var isRtf = value.Equals(RtfExtension, StringComparison.InvariantCultureIgnoreCase);

                CurrentFileDisplayType =
                    isRtf ? RichTextDescription : TextDescription;

                Document.CurrentFontName =
                    isRtf ? Settings.DefaultRtfFont : Settings.DefaultFont;
                Document.CurrentFontSize =
                    isRtf ? Settings.DefaultFontRtfSize : Settings.DefaultFontSize;
                Document.CurrentWordWrap =
                    isRtf ? Settings.RtfWordWrap : Settings.WordWrap;
                Document.GetOptions =
                    isRtf ? DEFAULT_TEXT_GET_OPTIONS_RTF : DEFAULT_TEXT_GET_OPTIONS_TXT;
                Document.SetOptions =
                    isRtf ? DEFAULT_TEXT_SET_OPTIONS_RTF : DEFAULT_TEXT_SET_OPTIONS_TXT;

                OnPropertyChanged(nameof(IsRtf));
            }
        }

        public string CurrentFileDisplayType
        {
            get => _currentFileDisplayType;
            set => Set(ref _currentFileDisplayType, value);
        }

        public event Action<float> SetScale;

        private float _scaleValue = 1;
        public float ScaleValue
        {
            get => _scaleValue;
            set
            {
                if (Set(ref _scaleValue, value))
                {
                    SetScale?.Invoke(value);
                }
            }
        }

        private string _fontColor;
        public string FontColor
        {
            get => _fontColor ??= Settings.DefaultTextForegroundColor;
            set => Set(ref _fontColor, value);
        }


        public void InvokeClearUndoRedo() => ClearUndoRedo?.Invoke();
        public Action ClearUndoRedo;

        public string FilePath
            => File?.Path;

        public void AddTab()
        {
            // Add tab
            if (SelectedText.Length > 0)
            {
                var currentPosition = CurrentPosition;
                var text = '\t' + SelectedText;
                text = text.Replace("\r", "\r\t").TrimEnd('\t');
                SelectedText = text;
                SelectText(currentPosition.start + 1, currentPosition.length);
            }
            else
            {
                SelectedText = "\t";
                SelectText(CurrentPosition.start + 1, 0);
            }
        }

        public string Title => ($" {(Document.IsDirty ? "*" : "")} {((File?.DisplayName) ?? Untitled)} ").Trim();

        private string _lastText;

        public void TextChanged<TArgs>(object sender, TArgs e)
        {
            var current = Text;

            if (_lastText == current) return;

            if (_lastText == null)
            {
                AreDifferent();
            }
            else if(_lastText.Length == current.Length)
            {
                if (_lastText.Where((t, i) => t != current[i]).Any())
                {
                    AreDifferent();
                }
            }
            else
            {
                AreDifferent();
            }

            void AreDifferent()
            {
                _lastText = current;

                Document.CalculateHash(current);

                Commands.NotifyAll(this, Settings);
            }
        }

        // Gets raw text from document, not formatted with control characters.
        public string Text
        {
            get => Document.Text ?? string.Empty;

            set
            {
                Document.Text = value;

                var text = Document.Text ?? string.Empty;

                if (Set(ref text, value))
                {
                    Document.CalculateHash();
                }
            }
        }

        public string SelectedText
        {
            get
            {
                var (start, length) = GetPosition?.Invoke() ?? default;

                if (length < 0)
                {
                    length = Math.Min(Math.Abs(length), Text.Length - start);
                    start = Math.Max(start - length, start);
                }

                return Text.Substring(start, length);
            }
            set => SetSelectedText?.Invoke(value);
        }

        public bool IsRtf => (CurrentFileType ?? Settings.DefaultFileType)
            .Equals(RtfExtension, StringComparison.CurrentCultureIgnoreCase);

        public (int start, int length) CurrentPosition
        {
            get => GetPosition?.Invoke() ?? (0, 0);
            set => SetSelection?.Invoke(value.start, value.length, false);
        }

        public event Func<(int start, int length)> GetPosition;
        public event Action<string> SetSelectedText;

        public void SelectText(int start, int length, bool reindex = true)
        {
            SetSelection?.Invoke(start, length, reindex);
        }

        public event Action Focus;

        public void SetFocus() => Focus?.Invoke();

        public event Action<int, int, bool> SetSelection;

        public StorageFileWrapper<TStorageFile> File
        {
            get => _file;
            set
            {
                if (!Set(ref _file, value)) return;

                if (!value?.FileType.Equals(CurrentFileType, StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    CurrentFileType = value.FileType;
                    CurrentFileDisplayType = value.DisplayType;
                }

                OnPropertyChanged(nameof(FilePath));
            }
        }

        public Encoding CurrentEncoding
        {
            get => _currentEncoding;
            set => Set(ref _currentEncoding, value);
        }

        public void ResetTimer()
        {
            if (Settings.AutoSave && File != null)
            {
                _timer.Change(TimeSpan.FromSeconds(Settings.AutoSaveFrequency), TimeSpan.FromMilliseconds(-1));
            }
        }

        private void AutoSaveTimer(object state)
        {
            if (Settings.AutoSave && File != null && Document.IsDirty)
            {
                Commands.SaveCommandBase.Execute(this);
            }
        }

        public async Task InitNewDocument()
        {
            HoldUpdates();

            File = null;

            SetEncoding(Settings.DefaultEncoding);
            CurrentFileType = Settings.DefaultFileType;

            try
            {
                if (IsRtf)
                {
                    Document.LoadFromStream(SetOption, null);
                }
                else
                {
                    Text = string.Empty;
                }

                CurrentPosition = (0, 0);
            }
            catch (Exception e)
            {
                Settings.Status(e.Message, TimeSpan.FromSeconds(60), Verbosity.Error);
            }
            finally
            {
                Document.IsDirty = false;

                ReleaseUpdates();
            }

            NotifyAll();
        }

        public string TextDescription { get; set; }

        public string RichTextDescription { get; set; }

        public Action<DocumentViewModel<TStorageFile, TStream>> Initialize { get; set; }
        public Action ExitApplication { get; set; }
        public bool Deferred { get; set; }

        public int CurrentLine
        {
            get => _currentLine;
            set => Set(ref _currentLine, value);
        }

        public int CurrentColumn
        {
            get => _currentColumn;
            set => Set(ref _currentColumn, value);
        }

        public int LineToGoTo
        {
            get => _lineToGoTo;
            set => Set(ref _lineToGoTo, value);
        }

        public bool CanUndo => Document.CanUndo;
        public bool CanRedo => Document.CanRedo;

        public bool ShowFind
        {
            get => _showFind;
            set => Set(ref _showFind, value);
        }

        private string _showReplaceIcon = ""; //this is the expand icon
        public string ShowReplaceIcon
        {
            get => _showReplaceIcon;
            set => Set(ref _showReplaceIcon, value);
        }

        public bool ShowReplace
        {
            get => _showReplace;
            set
            {
                if (Set(ref _showReplace, value) && value)
                {
                    ShowFind = true;
                    ShowReplaceIcon = ""; //this is the collapse button
                }
                else {ShowReplaceIcon = ""; } //this is the expand button
            }
        }

        public string RtfText
        {
            get
            {
                if (!IsRtf) return Text;

                Document.GetText(DEFAULT_TEXT_GET_OPTIONS_RTF, out var rtf);

                return rtf;
            }
            set
            {
                if (!IsRtf) Text = value;
                else if (value != RtfText)
                {
                    Document.SetText(DEFAULT_TEXT_SET_OPTIONS_RTF, value);

                    Document.CalculateHash();
                }
            }
        }

        public object Deferral { get; set; }
        public bool ShowClippy { get; set; }

        public void NotifyAll()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Document.IsDirty));
            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(CurrentEncoding));
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(GetOption));
            OnPropertyChanged(nameof(SetOption));
            OnPropertyChanged(nameof(Document.CurrentFontName));
            OnPropertyChanged(nameof(Document.CurrentFontSize));
            OnPropertyChanged(nameof(CurrentFileType));

            Commands.NotifyAll(this, Settings);
        }

        public void SetEncoding(string encoding)
        {
            CurrentEncoding = encoding switch
            {
                "UTF-8" => Encoding.UTF8,
                "UTF-16 LE" => Encoding.Unicode,
                "UTF-16 BE" => Encoding.BigEndianUnicode,
                "UTF-32" => Encoding.UTF32,
                "ASCII" => Encoding.ASCII,
                _ => Encoding.UTF8
            };
        }

        public event Action<DocumentViewModel<TStorageFile, TStream>> RedoRequested;
        public event Action<DocumentViewModel<TStorageFile, TStream>> UndoRequested;

        public void RequestUndo()
        {
            UndoRequested?.Invoke(this);
        }

        public void RequestRedo()
        {
            RedoRequested?.Invoke(this);
        }

        public void GoToLine(int lineToGoTo)
        {
            SelectText(lineToGoTo > 1 ? Document.LineIndices[lineToGoTo - 2] : 0, 0, false);
        }
    }
}
