using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class About : Page
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public SettingsViewModel Settings => App.Settings;
        public QuickPadCommands Commands => App.Commands;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
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
