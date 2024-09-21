using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.App.Helpers;
using QuickPad.App.Theme;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.App.Controls
{
	public sealed partial class FindAndReplaceView : UserControl
	{
		private DocumentViewModel<StorageFile, IRandomAccessStream> _viewModel;
		public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

		public WindowsSettingsViewModel Settings => App.Settings;

		public QuickPadCommands<StorageFile, IRandomAccessStream> Commands => App.Commands;

		public IFindAndReplaceView<StorageFile, IRandomAccessStream> FindReplaceViewModel => ViewModel?.FindAndReplaceViewModel;

		public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
		{
			get => _viewModel;
			set
			{
				if (_viewModel == value || value == null) return;

				_viewModel = value;

				DataContext = _viewModel.FindAndReplaceViewModel;

				App.Controller.AddView(FindReplaceViewModel);
			}
		}

		public FindAndReplaceView()
		{
			this.InitializeComponent();

			if (FindReplaceViewModel != null)
			{
				DataContext = FindReplaceViewModel;
			}
		}
	}
}