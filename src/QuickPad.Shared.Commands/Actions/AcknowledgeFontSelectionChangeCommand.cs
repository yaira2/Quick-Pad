using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class AcknowledgeFontSelectionChangeCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public AcknowledgeFontSelectionChangeCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.AcknowledgeFontSelectionChange = true;

                return Task.CompletedTask;
            };
        }
    }
}