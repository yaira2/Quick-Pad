using System;
using Windows.Storage;
using Windows.Storage.Streams;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Commands
{
    public class RateAndReviewCommand : SimpleCommand<SettingsViewModel<StorageFile, IRandomAccessStream>>, IRateAndReviewCommand<StorageFile, IRandomAccessStream>
    {
        public RateAndReviewCommand()
        {
            Executioner = async settings =>
            {
                var result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));
            };
        }
    }
}