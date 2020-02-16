using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement.Core;
using Microsoft.AppCenter.Analytics;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Commands
{
    public class EmojiCommand : SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>, IEmojiCommand<StorageFile, IRandomAccessStream>
    {
        public EmojiCommand()
        {
            Executioner = viewModel =>
            {
                viewModel.InvokeFocusTextBox();

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