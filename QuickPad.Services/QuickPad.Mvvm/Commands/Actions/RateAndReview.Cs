using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;
using Windows.Services.Store;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class RateAndReview : SimpleCommand<SettingsViewModel>
    {
        public RateAndReview()
        {
            Executioner = async settings =>
            {
                bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));
            };
        }
    }
}