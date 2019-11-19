using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace QuickPad.MVVM
{
    public class ShowSettings : SimpleCommand<DocumentViewModel>
    {
        public ShowSettings()
        {
            Executioner = viewModel =>
            {
                //open settings page
                _ = viewModel.Settings.ShowSettings != viewModel.Settings.ShowSettings;
                return Task.CompletedTask;
            };
        }
    }

}
