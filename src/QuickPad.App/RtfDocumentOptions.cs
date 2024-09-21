using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.App.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;

namespace QuickPad.App
{
    public class RtfDocumentOptions
    {
        public ITextDocument Document;
        public ILogger<RtfDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}