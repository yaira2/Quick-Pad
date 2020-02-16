using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowFontsCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
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