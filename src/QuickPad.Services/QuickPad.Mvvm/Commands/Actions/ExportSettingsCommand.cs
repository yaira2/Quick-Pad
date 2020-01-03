using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class ExportSettingsCommand : SimpleCommand<SettingsViewModel>
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