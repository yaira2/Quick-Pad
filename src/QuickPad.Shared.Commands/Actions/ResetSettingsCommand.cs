using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ResetSettingsCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
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