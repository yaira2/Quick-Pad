using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowMenusCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowMenusCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.CurrentMode = "LaunchClassicMode";

                return Task.CompletedTask;
            };
        }
    }
}