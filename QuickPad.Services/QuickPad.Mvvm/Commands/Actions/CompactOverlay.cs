using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;
using Windows.UI.ViewManagement;
using Windows.Foundation;


namespace QuickPad.Mvvm.Commands.Actions
{
    public class CompactOverlay : SimpleCommand<SettingsViewModel>
    {
        public CompactOverlay()
        {
            Executioner = async settings =>
            {
                if (settings.CurrentMode == nameof(DisplayModes.LaunchCompactOverlay)) //the current mode is already CompactOverlay
                {
                    settings.CurrentMode = settings.ReturnToMode; //set the current mode the previous one
                    var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default); //switch to the correct mode
                }
                else
                {
                    settings.ShowCompactOverlayTip = false; //hide the CompactOverlayTip if its visible
                    settings.CurrentMode = nameof(DisplayModes.LaunchCompactOverlay); //change the mode to CompactOverlay
                    var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay); //switch to the correct mode
                }
            };
        }
    }
}