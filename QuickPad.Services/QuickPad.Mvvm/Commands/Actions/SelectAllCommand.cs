using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class SelectAllCommand : SimpleCommand<DocumentViewModel>
    {
        public SelectAllCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.SelectText(0, documentViewModel.Text.Length, false);
                return Task.CompletedTask;
            };
        }
    }
}