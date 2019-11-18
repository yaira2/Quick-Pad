using System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Microsoft.AppCenter.Analytics;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVVM.Commands.Editing
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
                viewModel.InvokeFocusTextBox(FocusState.Programmatic);

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