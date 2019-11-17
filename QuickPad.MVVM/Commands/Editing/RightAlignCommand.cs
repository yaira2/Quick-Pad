using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class RightAlignCommand : SimpleCommand<DocumentViewModel>
    {
        public RightAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Right;
                return Task.CompletedTask;
            };
        }
    }

}
