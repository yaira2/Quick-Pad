using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace QuickPad.UI
{
    public class TextDocumentOptions
    {
        public TextBox Document;
        public ILogger<TextDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}