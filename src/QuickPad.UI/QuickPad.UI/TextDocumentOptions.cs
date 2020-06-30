using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;

namespace QuickPad.UI
{
    public class TextDocumentOptions
    {
        public TextBox Document;
        public ILogger<TextDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}