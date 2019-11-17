using Microsoft.AppCenter.Analytics;
using QuickPad.Mvvm;
using QuickPad.MVVM.Commands;
using System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;

namespace QuickPad.MVVM
{
    /*
     *

    */

    public class EmojiCommand : SimpleCommand<DocumentViewModel>
    {
        public EmojiCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.InvokeFocusTextbox(FocusState.Programmatic);

                try //More error here
                {
                    CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji);
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent($"Attempting to open emoji keyboard\r\n{ex.Message}");
                }

                return Task.CompletedTask;
            };
        }
    }

}
