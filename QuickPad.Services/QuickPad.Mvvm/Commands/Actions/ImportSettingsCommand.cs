using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class ImportSettingsCommand : SimpleCommand<SettingsViewModel>
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