using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShareCommand : SimpleCommand<DocumentViewModel>
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