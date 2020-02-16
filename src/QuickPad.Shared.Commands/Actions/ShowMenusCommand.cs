using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ShowMenusCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
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