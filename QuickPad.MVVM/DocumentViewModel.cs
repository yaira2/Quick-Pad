using Microsoft.Extensions.Logging;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace QuickPad.Mvvm
{
    public class DocumentViewModel : ViewModel
    {
        private ITextDocument _document = null;

        private Encoding _encoding;
        private HMAC _md5;

        private string _originalHash;
        private string _currentHash;

        private StorageFile _file;


        public DocumentViewModel(ILogger<DocumentViewModel> logger) : base(logger)
        {
            _md5 = HMACMD5.Create("HMACMD5");
            _md5.Key = UnicodeEncoding.ASCII.GetBytes("12345");
        }

        public ITextDocument Document
        {
            get => _document;
            set
            {
                Set(ref _document, value);
            }
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
            var hash = _md5.ComputeHash(UnicodeEncoding.ASCII.GetBytes(Text));

            _currentHash = UnicodeEncoding.ASCII.GetString(hash);

            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(Title));
        }

        public SimpleCommand<DocumentViewModel> NewCommand { get; set; }
        public SimpleCommand<DocumentViewModel> LoadCommand { get; set; }
        public SimpleCommand<DocumentViewModel> SaveCommand { get; set; }
        public SimpleCommand<DocumentViewModel> SaveAsCommand { get; set; }
        public SimpleCommand<DocumentViewModel> PrintCommand { get; set; }
        public SimpleCommand<DocumentViewModel> ShareCommand { get; set; }
        public SimpleCommand<DocumentViewModel> ExitCommand { get; set; }

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
