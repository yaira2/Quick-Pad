using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.App.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace QuickPad.App
{
    public class TextDocumentOptions
    {
        public TextBox Document;
        public ILogger<TextDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}