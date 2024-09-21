using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.App.Helpers;
using System;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;

namespace QuickPad.App.Commands
{
    public class CompactOverlayCommand : SimpleCommand<SettingsViewModel<StorageFile, IRandomAccessStream>>, ICompactOverlayCommand<StorageFile, IRandomAccessStream>
    {
        public CompactOverlayCommand()
        {
            Executioner = async settings =>
            {
                var windowsSettings = settings as WindowsSettingsViewModel;

                if (settings.CurrentMode == nameof(DisplayModes.LaunchCompactOverlay)) //the current mode is already CompactOverlayCommand
                {
                    settings.CurrentMode = settings.ReturnToMode; //set the current mode the previous one
                    var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default); //switch to the correct mode
                }
                else
                {
                    windowsSettings.ShowCompactOverlayTip = false; //hide the CompactOverlayTip if its visible
                    settings.CurrentMode = nameof(DisplayModes.LaunchCompactOverlay); //change the mode to CompactOverlayCommand
                    var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay); //switch to the correct mode
                }
            };
        }
    }
}