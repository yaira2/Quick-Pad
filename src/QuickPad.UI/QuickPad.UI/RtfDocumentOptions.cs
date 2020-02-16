using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;

namespace QuickPad.UI
{
    public class RtfDocumentOptions
    {
        public ITextDocument Document;
        public ILogger<RtfDocument> Logger;
        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel;
    }
}