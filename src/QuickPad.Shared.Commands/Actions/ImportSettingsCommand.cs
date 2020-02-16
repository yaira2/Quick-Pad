using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ImportSettingsCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public ImportSettingsCommand()
        {
            Executioner = async settings =>
            {
                //open settings page
                await settings.ImportSettings();
            };
        }
    }
}