using System;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace QuickPad.MVVM.ViewModels
{
    public class DocumentViewModel : ViewModel
    {
        private ITextDocument _document;

        private Encoding _encoding;
        private readonly HMAC _md5;

        private string _originalHash;
        private string _currentHash;

        private StorageFile _file;
        
        public DocumentViewModel(ILogger<DocumentViewModel> logger, IServiceProvider serviceProvider) : base(logger)
        {
            _md5 = HMAC.Create("HMACMD5");
            _md5.Key = Encoding.ASCII.GetBytes("12345");
            ServiceProvider = serviceProvider;
        }

        private IServiceProvider ServiceProvider { get; }

        public SettingsViewModel Settings => ServiceProvider.GetService<SettingsViewModel>();

        public void InvokeFocusTextBox(FocusState focusState) => RequestFocusTextBox?.Invoke(focusState);
        
        public event Action<FocusState> RequestFocusTextBox;
        public ITextDocument Document
        {
            get => _document;
            set => Set(ref _document, value);
        }

        public TextGetOptions GetOption { get; set; } = TextGetOptions.None;
        public TextSetOptions SetOption { get; set; } = TextSetOptions.None;

        public bool IsDirty
        {
            get => (_originalHash != _currentHash);
            set
            {
                if (!value)
                {
                    _originalHash = _currentHash;                    
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => ($" {(IsDirty ? "*" : "")} {((File?.DisplayName) ?? "")} ").Trim();

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
        }

        public StorageFile File
        {
            get => _file;
            set => Set(ref _file, value);
        }

        public Encoding Encoding
        {
            get => _encoding;
            set => Set(ref _encoding, value);
        }

        public void InitNewDocument()
        {
            Initialize?.Invoke(this);
        }

        public Action<DocumentViewModel> Initialize { get; set; }

        public void NotifyAll()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Document));
            OnPropertyChanged(nameof(Encoding));
            OnPropertyChanged(nameof(File));
            OnPropertyChanged(nameof(GetOption));
            OnPropertyChanged(nameof(SetOption));
        }
    }
}
