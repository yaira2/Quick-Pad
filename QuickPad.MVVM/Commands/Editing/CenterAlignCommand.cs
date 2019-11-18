using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class CenterAlignCommand : SimpleCommand<DocumentViewModel>
    {
        public CenterAlignCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                return Task.CompletedTask;
            };
        }
    }

}
