using QuickPad.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Clipboard
{
    public class SearchWithBingCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public SearchWithBingCommand()
        {
            CanExecuteEvaluator = settingsViewModel => settingsViewModel.CurrentViewModel.SelectedText.Length > 0;

            Executioner = viewModel =>
            {
                viewModel.LaunchUri(new Uri($"https://www.bing.com/search?q={Uri.EscapeDataString(viewModel.CurrentViewModel.SelectedText)}"));

                return Task.CompletedTask;
            };
        }
    }
}