using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.App.Helpers;
using QuickPad.App.Theme;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.App.Controls.Settings
{
    public sealed partial class About : Page
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public WindowsSettingsViewModel Settings => App.Settings;
        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands => App.Commands;

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => DataContext as DocumentViewModel<StorageFile, IRandomAccessStream>;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
            }
        }

        public About()
        {
            this.InitializeComponent();
        }
    }
}