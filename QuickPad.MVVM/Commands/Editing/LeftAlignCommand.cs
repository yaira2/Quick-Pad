using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class LeftAlignCommand : SimpleCommand<DocumentViewModel>
    {
        public LeftAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                return Task.CompletedTask;
            };
        }
    }

}
