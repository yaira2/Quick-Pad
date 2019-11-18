using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace QuickPad.MVVM
{
    public class BulletsCommand : SimpleCommand<DocumentViewModel>
    {
        public BulletsCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.Document.BeginUndoGroup();
                if (viewModel.Document.Selection.ParagraphFormat.ListType == MarkerType.Bullet)
                {
                    viewModel.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
                }
                else
                {
                    viewModel.Document.Selection.ParagraphFormat.ListType = MarkerType.Bullet;
                }
                viewModel.Document.EndUndoGroup();

                return Task.CompletedTask;
            };
        }
    }

}
