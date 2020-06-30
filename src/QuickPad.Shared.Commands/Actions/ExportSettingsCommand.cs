using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ExportSettingsCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ExportSettingsCommand()
        {
            Executioner = async settings =>
            {
                //open settings page
                await settings.ExportSettings();
            };
        }
    }
}