using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class DocumentViewModel : ViewModel
    {
        private ITextDocument _document = null;

        public ITextDocument Document
        {
            get => _document;
            set => Set(ref _document, value);
        }

        public TextGetOptions GetOption { get; set; } = TextGetOptions.None;
        public TextSetOptions SetOption { get; set; } = TextSetOptions.None;

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

                var text = string.Empty;
                Set(ref text, value);
            }
        }

        public SimpleCommand<DocumentViewModel> SaveCommand { get; set; }

        private StorageFile _file;

        public StorageFile File
        {
            get => _file;
            set => Set(ref _file, value);
        }

        private Encoding _encoding;
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
    }
}
