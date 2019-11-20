using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using QuickPad.Mvvm;
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
        private double _currentFontSize;
        private string _currentFileType;
        private bool _currentWordWrap;

        private SettingsViewModel Settings { get; }

        public DocumentViewModel(ILogger<DocumentViewModel> logger
            , IServiceProvider serviceProvider
            , SettingsViewModel settings) : base(logger)
        {
            _md5 = HMAC.Create("HMACMD5");
            _md5.Key = Encoding.ASCII.GetBytes("12345");
            ServiceProvider = serviceProvider;

            Settings = settings;
        }

        private IServiceProvider ServiceProvider { get; }

        //public SettingsViewModel Settings => ServiceProvider.GetService<SettingsViewModel>();

        public void InvokeFocusTextBox(FocusState focusState) => RequestFocusTextBox?.Invoke(focusState);
        
        public event Action<FocusState> RequestFocusTextBox;
        public ITextDocument Document
        {
            get => _document;
            set => Set(ref _document, value);
        }

        public TextGetOptions GetOption { get; set; } = TextGetOptions.None;
        public TextSetOptions SetOption { get; set; } = TextSetOptions.None;

        public string CurrentFileType
        {
            get => _currentFileType;
            set => Set(ref _currentFileType, value);
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

        public Deferral Deferral { get; set; }

        public string CurrentFontName
        {
            get => _currentFontName;
            set => Set(ref _currentFontName, value);
        }

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
            CalculateHash();
        }

        public string Text
        {
            get
            {
                var text = string.Empty;

                Document?.GetText(GetOption, out text);

                return text;
            }
            set
            {
                Document?.SetText(SetOption, value);

                CalculateHash();

                var text = string.Empty;
                Set(ref text, value);
            }
        }

        private void CalculateHash()
        {
            var hash = _md5.ComputeHash(Encoding.ASCII.GetBytes(Text));

            _currentHash = Encoding.ASCII.GetString(hash);

            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Text));
        }

        public StorageFile File
        {
            get => _file;
            set => Set(ref _file, value);
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
            CurrentFontName =
                CurrentFileType == ".rtf" ? Settings.DefaultRtfFont : Settings.DefaultFont;
            CurrentFontSize =
                CurrentFileType == ".rtf" ? Settings.DefaultFontRtfSize : Settings.DefaultFontSize;
            CurrentWordWrap =
                CurrentFileType == ".rtf" ? Settings.RtfWordWrap : Settings.WordWrap;

            var memoryStream = new InMemoryRandomAccessStream();
            
            var bytes = Encoding.UTF8.GetBytes("\r");
            var buffer = bytes.AsBuffer();

            await memoryStream.ReadAsync(buffer, buffer.Length, InputStreamOptions.None);

            Document?.LoadFromStream(SetOption, memoryStream);

            IsDirty = false;

            ReleaseUpdates();

            NotifyAll();
        }

        public Action<DocumentViewModel> Initialize { get; set; }
        public Action ExitApplication { get; set; }

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
    }
}
