using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class HideFindCommand : SimpleCommand<DocumentViewModel>
    {
        public HideFindCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowFind = false;
                return Task.CompletedTask;
            };
        }
    }
}