using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;
using QuickPad.UI.Common.Theme;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class FindAndReplace : UserControl
    {
        public VisualThemeSelector VisualThemeSelector => VisualThemeSelector.Current;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
            }
        }

        public FindAndReplace()
        {
            this.InitializeComponent();
        }

    }
}
