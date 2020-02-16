using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowSettingsCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ShowSettingsCommand()
        {
            Executioner = settings =>
            {
                //open settings page
                settings.ShowSettings = !settings.ShowSettings;

                return Task.CompletedTask;
            };
        }
    }
}