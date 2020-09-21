using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;

namespace QuickPad.UI
{
    public class RtfDocumentOptions
    {
        public ITextDocument Document;
        public ILogger<RtfDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}