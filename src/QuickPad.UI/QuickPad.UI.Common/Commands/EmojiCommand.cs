using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Microsoft.AppCenter.Analytics;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Editing
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