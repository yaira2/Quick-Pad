using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowCommandBarCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowCommandBarCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.CurrentMode = "LaunchDefaultMode";

                return Task.CompletedTask;
            };
        }
    }
    public class ShowStatusBarCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowStatusBarCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.StatusBar = !settings.StatusBar;

                return Task.CompletedTask;
            };
        }
    }

}