using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowFontsCommand : SimpleCommand<SettingsViewModel>
    {
        public ShowFontsCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.ShowSettings = true;
                settings.ShowSettingsTab = SettingsTabs.Fonts;

                return Task.CompletedTask;
            };
        }
    }
}