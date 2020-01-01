using System;
using Windows.System;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Clipboard
{
    public class GoogleCommand : SimpleCommand<DocumentViewModel>
    {
        private const string PATH = "https://google.com/search?q=";

        public GoogleCommand()
        {
            CanExecuteEvaluator = viewModel => viewModel.SelectedText.Length > 0;

            Executioner = async viewModel =>
            {
                if (!string.IsNullOrWhiteSpace(viewModel.SelectedText))
                {
                    var uri = new Uri(PATH + @Uri.EscapeDataString(viewModel.SelectedText));
                    var success = await Launcher.LaunchUriAsync(uri);
                }
            };
        }
    }
}