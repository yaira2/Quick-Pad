using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class JustifyCommand : SimpleCommand<DocumentViewModel>
    {
        public JustifyCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
                return Task.CompletedTask;
            };
        }
    }

}
