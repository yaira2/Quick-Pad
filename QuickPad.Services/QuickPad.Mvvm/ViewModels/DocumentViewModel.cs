using System;
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

namespace QuickPad.Mvvm.ViewModels
{
    public class DocumentViewModel : ViewModel
    {
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
        private string _selectedText;

        public DocumentViewModel(ILogger<DocumentViewModel> logger
            , IServiceProvider serviceProvider
            , SettingsViewModel settings) : base(logger)
        {
            _md5 = HMAC.Create("HMACMD5");
            _md5.Key = Encoding.ASCII.GetBytes("12345");
            ServiceProvider = serviceProvider;

            Settings = settings;

            _resourceLoader = ResourceLoader.GetForCurrentView();

            RichTextDescription = _resourceLoader.GetString("RichTextDescription");
            TextDescription = _resourceLoader.GetString("TextDescription");
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
                    var isRtf = value == ".rtf";

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
                }
            }
        }

        public string CurrentFileDisplayType
        {
            get => _currentFileDisplayType;
            set => Set(ref _currentFileDisplayType, value);
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
            get => _currentWordWrap;
            set => Set(ref _currentWordWrap, value);
        }

        public string Title => ($" {(IsDirty ? "*" : "")} {((File?.DisplayName) ?? "Untitled")} ").Trim();

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

        public string Text
        {
            get
            {
                if (CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    Document?.GetText(GetOption, out _text);
                }

                return _text ??= string.Empty;
            }
            set
            {
                if(CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    Document?.SetText(SetOption, value);
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
                if (CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    Document?.Selection.GetText(GetOption, out _selectedText);
                }

                return _selectedText ??= string.Empty;
            }
            set
            {
                if (CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase))
                {
                    Document?.Selection.SetText(SetOption, value);
                }

                Set(ref _selectedText, value);
            }
        }

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

        public async Task InitNewDocument()
        {
            HoldUpdates();

            File = null;

            SetEncoding(Settings.DefaultEncoding);
            CurrentFileType = Settings.DefaultFileType;

            try
            {
                var memoryStream = new InMemoryRandomAccessStream();
                var bytes = Encoding.UTF8.GetBytes("\r");
                var buffer = bytes.AsBuffer();
                await memoryStream.WriteAsync(buffer);

                memoryStream.Seek(0);
                Document?.LoadFromStream(SetOption, memoryStream);
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
            get => CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase) ? Document.CanUndo() : _canUndo;
            set => Set(ref _canUndo, value);
        }

        public bool CanRedo {
            get => CurrentFileType.Equals(".rtf", StringComparison.InvariantCultureIgnoreCase) ? Document.CanRedo() : _canRedo;
            set => Set(ref _canRedo, value);
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
