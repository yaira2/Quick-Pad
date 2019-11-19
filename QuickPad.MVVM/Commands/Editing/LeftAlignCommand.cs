using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
{
    public class LeftAlignCommand : SimpleCommand<DocumentViewModel>
    {
        public LeftAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}