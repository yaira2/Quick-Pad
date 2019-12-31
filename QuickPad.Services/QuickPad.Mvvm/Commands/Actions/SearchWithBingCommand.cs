using QuickPad.Mvvm.ViewModels;
using System;

namespace QuickPad.Mvvm.Commands.Clipboard
{

    public class SearchWithBingCommand : SimpleCommand<DocumentViewModel>
    {
        public SearchWithBingCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = async viewModel =>
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"https://www.bing.com/search?q={viewModel.SelectedText}"));
            };
        }
    }
}