using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using Buffer = Windows.Storage.Streams.Buffer;
using System.Timers;
using System.Threading;
using QuickPad.Mvvm.Views;
using Timer = System.Threading.Timer;
using Windows.UI;
using QuickPad.Mvvm.Models.Theme;

namespace QuickPad.Mvvm.ViewModels
{
    public class DocumentViewModel : ViewModel
    {
        public const string RTF_EXTENSION = ".rtf";
        private ITextDocument _document;

        private Encoding _currentEncoding;
        private readonly HMAC _md5;

        private string _originalHash;
        private string _currentHash;

        private StorageFile _file;
        private string _currentFontName;
        private double _currentFontSize = 14;
        private string _currentFileType;
        private bool _currentWordWrap;
        private string _currentFileDisplayType;
        private ResourceLoader _resourceLoader;
        private int _currentColumn;
        private int _currentLine;

        private string _text;
        private bool _canUndo;
        private bool _canRedo;

        private readonly Timer timer;
        private IFindAndReplaceView _findAndReplaceViewModel;
        private bool _showFind;
        private bool _showReplace;

        public DocumentViewModel(ILogger<DocumentViewModel> logger
            , IFindAndReplaceView findAndReplaceViewModel
            , IServiceProvider serviceProvider
            , SettingsViewModel settings) : base(logger)
        {
            _md5 = HMAC.Create("HMACMD5");
            _md5.Key = Encoding.ASCII.GetBytes("12345");
            ServiceProvider = serviceProvider;
            FindAndReplaceViewModel = findAndReplaceViewModel;

            Settings = settings;

            _resourceLoader = ResourceLoader.GetForCurrentView();

            RichTextDescription = _resourceLoader.GetString("RichTextDescription");
            TextDescription = _resourceLoader.GetString("TextDescription");
            Untitled = _resourceLoader.GetString("Untitled");

            timer = new Timer(AutoSaveTimer);
        }

        public static string Untitled { get; private set; }

        public IFindAndReplaceView FindAndReplaceViewModel
        {
            get => _findAndReplaceViewModel;
            set => Set(ref _findAndReplaceViewModel, value);
        }

        private SettingsViewModel Settings { get; }

        private IServiceProvider ServiceProvider { get; }

        //public SettingsViewModel Settings => ServiceProvider.GetService<SettingsViewModel>();

        public void InvokeFocusTextBox(FocusState focusState) => RequestFocusTextBox?.Invoke(focusState);

        public event Action<FocusState> RequestFocusTextBox;
        public ITextDocument Document
        {
            get => _document;
            set => Set(ref _document, value);
        }

        private const TextGetOptions DEFAULT_TEXT_GET_OPTIONS_TXT = TextGetOptions.None | TextGetOptions.AdjustCrlf | TextGetOptions.NoHidden | TextGetOptions.UseCrlf;
        private const TextSetOptions DEFAULT_TEXT_SET_OPTIONS_TXT = TextSetOptions.None | TextSetOptions.Unhide;

        private const TextGetOptions DEFAULT_TEXT_GET_OPTIONS_RTF =
            TextGetOptions.FormatRtf | TextGetOptions.IncludeNumbering | TextGetOptions.NoHidden;
        private const TextSetOptions DEFAULT_TEXT_SET_OPTIONS_RTF = TextSetOptions.FormatRtf | TextSetOptions.Unhide;


        public TextGetOptions GetOption { get; private set; } = DEFAULT_TEXT_GET_OPTIONS_TXT;
        public TextSetOptions SetOption { get; private set; } = DEFAULT_TEXT_SET_OPTIONS_TXT;

        public string CurrentFileType
        {
            get => _currentFileType ?? Settings.DefaultFileType;
            set
            {
                if (Set(ref _currentFileType, value))
                {
                    var isRtf = value.Equals(RTF_EXTENSION, StringComparison.InvariantCultureIgnoreCase);

                    CurrentFileDisplayType =
                        isRtf ? RichTextDescription : TextDescription;
                    CurrentFontName =
                        isRtf ? Settings.DefaultRtfFont : Settings.DefaultFont;
                    CurrentFontSize =
                        isRtf ? Settings.DefaultFontRtfSize : Settings.DefaultFontSize;
                    CurrentWordWrap =
                        isRtf ? Settings.RtfWordWrap : Settings.WordWrap;
                    GetOption =
                        isRtf ? DEFAULT_TEXT_GET_OPTIONS_RTF : DEFAULT_TEXT_GET_OPTIONS_TXT;
                    SetOption =
                        isRtf ? DEFAULT_TEXT_SET_OPTIONS_RTF : DEFAULT_TEXT_SET_OPTIONS_TXT;

                    OnPropertyChanged(nameof(IsRtf));
                }
            }
        }

        public string CurrentFileDisplayType
        {
            get => _currentFileDisplayType;
            set => Set(ref _currentFileDisplayType, value);
        }

        private bool _selBold;
        public bool SelBold
        {
            get => _selBold;
            set => Set(ref _selBold, value);
        }

        private bool _selItalic;
        public bool SelItalic
        {
            get => _selItalic;
            set => Set(ref _selItalic, value);
        }

        private bool _selUnderline;
        public bool SelUnderline
        {
            get => _selUnderline;
            set => Set(ref _selUnderline, value);
        }

        private bool _selStrikethrough;
        public bool SelStrikethrough
        {
            get => _selStrikethrough;
            set => Set(ref _selStrikethrough, value);
        }

        private bool _selCenter;
        public bool SelCenter
        {
            get => _selCenter;
            set => Set(ref _selCenter, value);
        }

        private bool _selRight;
        public bool SelRight
        {
            get => _selRight;
            set => Set(ref _selRight, value);
        }

        private bool _selLeft;
        public bool SelLeft
        {
            get => _selLeft;
            set => Set(ref _selLeft, value);
        }

        private bool _selJustify;
        public bool SelJustify
        {
            get => _selJustify;
            set => Set(ref _selJustify, value);
        }

        private bool _selBullets;
        public bool SelBullets
        {
            get => _selBullets;
            set => Set(ref _selBullets, value);
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

        private Color? _fontColor;
        public Color FontColor
        {
            get => _fontColor ??= Settings.DefaultTextForegroundColor;
            set => Set(ref _fontColor, value);
        }

        public bool IsDirty
        {
            get => (_originalHash != _currentHash);
            set
            {
                if (!value)
                {
                    CalculateHash(Text);
                    _originalHash = _currentHash;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public void InvokeClearUndoRedo() => ClearUndoRedo?.Invoke();
        public Action ClearUndoRedo;

        public Deferral Deferral { get; set; }

        public string CurrentFontName
        {
            get => _currentFontName ?? Settings.DefaultFont;
            set => Set(ref _currentFontName, value);
        }

        public string FilePath
            => File?.Path;

        public double CurrentFontSize
        {
            get => _currentFontSize;
            set => Set(ref _currentFontSize, value);
        }

        public bool CurrentWordWrap
        {
            get => IsRtf ? Settings.RtfWordWrap : Settings.WordWrap;
            set
            {
                if (IsRtf)
                {
                    Settings.RtfWordWrap = value;
                }
                else
                {
                    Settings.WordWrap = value;
                }
            }
        }

        public async Task AddTab()
        {
            // Add tab
            if (SelectedText.Length > 0)
            {
                var currentPosition = CurrentPosition;
                var text = '\t' + SelectedText;
                text = text.Replace("\r", "\r\t").TrimEnd('\t');
                SelectedText = text;
                await SelectText(currentPosition.start + 1, currentPosition.length);
            }
            else
            {
                SelectedText = "\t";
                await SelectText(CurrentPosition.start + 1, 0);
            }
        }


        public string Title => ($" {(IsDirty ? "*" : "")} {((File?.DisplayName) ?? Untitled)} ").Trim();

        public void TextChanged(object sender, RoutedEventArgs e)
        {
            CalculateHash(Text);

            if (sender is TextBox textBox)
            {
                CanUndo = textBox.CanUndo;
                CanRedo = textBox.CanRedo;
            }

            QuickPadCommands.NotifyAll(this, Settings);
        }

        // Gets raw text from document, not formatted with control characters.
        public string Text
        {
            get
            {
                if (IsRtf)
                {
                    Document?.GetText(DEFAULT_TEXT_GET_OPTIONS_TXT, out _text);
                }

                return _text ??= string.Empty;
            }
            set
            {
                if(!IsRtf)
                {
                    Document?.SetText(DEFAULT_TEXT_SET_OPTIONS_TXT, value);
                }

                if (Set(ref _text, value))
                {
                    CalculateHash(_text);
                }
            }
        }

        public string SelectedText
        {
            get
            {
                var position = GetPosition?.Invoke() ?? default;

                if (position.length < 0)
                {
                    position.length = Math.Min(Math.Abs(position.length), Text.Length - position.start);
                    position.start = Math.Max(position.start - position.length, position.start);
                }

                return Text.Substring(position.start, position.length);
            }
            set => SetSelectedText?.Invoke(value);
        }

        public bool IsRtf => (CurrentFileType ?? Settings.DefaultFileType)
            .Equals(RTF_EXTENSION, StringComparison.CurrentCultureIgnoreCase);

        public (int start, int length) CurrentPosition
        {
            get => GetPosition?.Invoke() ?? (0, 0);
            set => SetSelection?.Invoke(value.start, value.length);
        }

        public event Func<(int start, int length)> GetPosition;
        public event Action<string> SetSelectedText;

        public async Task SelectText(int start, int length)
        {
            if (SetSelection != null)
            {
                await SetSelection?.Invoke(start, length);
            }
        }

        public event Func<int, int, Task> SetSelection;

        private void CalculateHash(string text)
        {
            var hash = _md5.ComputeHash(Encoding.ASCII.GetBytes(text ?? string.Empty));

            var newHash = Encoding.ASCII.GetString(hash);

            if (newHash != _currentHash)
            {
                _currentHash = newHash;

                OnPropertyChanged(nameof(IsDirty));
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Text));
            }
        }

        public StorageFile File
        {
            get => _file;
            set
            {
                if (Set(ref _file, value))
                {
                    if (!value?.FileType.Equals(CurrentFileType, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        CurrentFileType = value.FileType;
                        CurrentFileDisplayType = value.DisplayType;
                    }

                    OnPropertyChanged(nameof(FilePath));
                }
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
                timer.Change(TimeSpan.FromSeconds(Settings.AutoSaveFrequency), TimeSpan.FromMilliseconds(-1));
            }
        }

        private void AutoSaveTimer(object state)
        {
            if (Settings.AutoSave && this.File != null && IsDirty == true)
            {
                ServiceProvider.GetService<QuickPadCommands>().SaveCommand.Execute(this);
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
                    var memoryStream = new InMemoryRandomAccessStream();
                    var bytes = Encoding.UTF8.GetBytes("\r");
                    var buffer = bytes.AsBuffer();
                    await memoryStream.WriteAsync(buffer);

                    memoryStream.Seek(0);

                    Document?.LoadFromStream(SetOption, memoryStream);
                }
                else
                {
                    Text = string.Empty;
                }

                CurrentPosition = (0, 0);
            }
            catch (Exception e)
            {
                Settings.Status(e.Message, TimeSpan.FromSeconds(60), SettingsViewModel.Verbosity.Error);
            }
            finally
            {
                IsDirty = false;

                ReleaseUpdates();
            }

            NotifyAll();
        }

        public string TextDescription { get; set; } = "Text Document";

        public string RichTextDescription { get; set; } = "Rich Text Document";

        public Action<DocumentViewModel> Initialize { get; set; }
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

        public bool CanUndo
        {
            get => IsRtf ? Document.CanUndo() : _canUndo;
            set => Set(ref _canUndo, value);
        }

        public bool CanRedo
        {
            get => IsRtf ? Document.CanRedo() : _canRedo;
            set => Set(ref _canRedo, value);
        }

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

                    CalculateHash(_text);
                }
            }
        }

        public void NotifyAll()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(CurrentEncoding));
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(GetOption));
            OnPropertyChanged(nameof(SetOption));
            OnPropertyChanged(nameof(CurrentFontName));
            OnPropertyChanged(nameof(CurrentFontSize));
            OnPropertyChanged(nameof(CurrentFileType));

            QuickPadCommands.NotifyAll(this, Settings);
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

        public void RequestUndo()
        {
            UndoRequested?.Invoke(this);
        }

        public event Action<DocumentViewModel> UndoRequested;

        public void RequestRedo()
        {
            RedoRequested?.Invoke(this);
        }

        public event Action<DocumentViewModel> RedoRequested;
    }
}
