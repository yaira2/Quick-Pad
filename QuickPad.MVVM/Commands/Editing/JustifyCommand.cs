using System.Threading.Tasks;
using Windows.UI.Text;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVVM.Commands.Editing
{
    public class JustifyCommand : SimpleCommand<DocumentViewModel>
    {
        public JustifyCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
                viewModel.OnPropertyChanged(nameof(viewModel.Text));

                return Task.CompletedTask;
            };
        }
    }
}