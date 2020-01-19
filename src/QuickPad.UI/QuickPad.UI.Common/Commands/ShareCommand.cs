using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
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