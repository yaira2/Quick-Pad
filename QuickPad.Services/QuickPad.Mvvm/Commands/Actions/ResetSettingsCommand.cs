using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ResetSettingsCommand : SimpleCommand<SettingsViewModel>
    {
        public ResetSettingsCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.ResetSettings();

                return Task.CompletedTask;
            };
        }
    }
}