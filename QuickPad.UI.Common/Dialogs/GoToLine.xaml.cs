using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.UI.Common.Theme;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class GoToLine : ContentDialog
    {
        public IVisualThemeSelector VTSelector { get; }

        public GoToLine(IVisualThemeSelector vts)
        {
            VTSelector = vts;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
