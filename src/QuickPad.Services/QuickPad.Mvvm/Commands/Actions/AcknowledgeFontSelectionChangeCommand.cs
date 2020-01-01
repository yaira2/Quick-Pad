using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class AcknowledgeFontSelectionChangeCommand : SimpleCommand<SettingsViewModel>
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