using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement.Core;

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
				catch (Exception)
				{
				}

				return Task.CompletedTask;
			};
		}
	}
}