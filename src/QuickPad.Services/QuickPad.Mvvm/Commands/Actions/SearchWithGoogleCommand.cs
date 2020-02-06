using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Clipboard
{

    public class SearchWithGoogleCommand : SimpleCommand<DocumentViewModel>
    {
        public SearchWithGoogleCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = async viewModel =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"https://www.google.com/search?q={Uri.EscapeDataString(viewModel.SelectedText)}"));
            };
        }
    }
}