using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVVM.Commands.Editing
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