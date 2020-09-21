using QuickPad.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Clipboard
{
    public class SearchWithGoogleCommand<TStorageFile, TStream> : SimpleCommand<SettingsViewModel<TStorageFile, TStream>>
        where TStream : class
    {
        public SearchWithGoogleCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.CurrentViewModel.SelectedText.Length > 0;

            Executioner = viewModel =>
            {
                viewModel.LaunchUri(new Uri($"https://www.google.com/search?q={Uri.EscapeDataString(viewModel.CurrentViewModel.SelectedText)}"));

                return Task.CompletedTask;
            };
        }
    }
}