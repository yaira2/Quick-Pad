using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace QuickPad.App.Commands
{
    public class ShareCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, IShareCommand<StorageFile, IRandomAccessStream>
    {
        public ShareCommand()
        {
            Executioner = viewModel =>
            {
                DataTransferManager.ShowShareUI();
                return Task.CompletedTask;
            };
        }
    }
}