using Quick_Pad_Free_Edition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.Dialog
{
    public sealed partial class SaveChange : ContentDialog
    {
        public DialogResult DialogResult { get; set; } = DialogResult.None;
        public static bool IsOpen { get; set; }
        public SaveChange()
        {
            this.InitializeComponent();
        }
        
        private void Yes(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Yes;
            this.Hide();
        }

        private void No(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.No;
            this.Hide();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //Reset value
            DialogResult = DialogResult.None;
            //
            SaveDialogYes.Focus(FocusState.Keyboard);
            IsOpen = true;
        }

        private void ContentDialog_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            //Incase of "Esc"
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Hide();
            }
        }

        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            IsOpen = true;
        }
    }
}
