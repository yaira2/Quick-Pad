using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class FocusCommand : SimpleCommand<SettingsViewModel>
    {
        public FocusCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.CurrentMode = "Focus Mode";

                return Task.CompletedTask;
            };
        }
    }
}