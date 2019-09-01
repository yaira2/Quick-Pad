using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.Dialog
{
    public sealed partial class GoTo : ContentDialog
    {
        public GoTo()
        {
            this.InitializeComponent();
        }

        public DialogResult finalResult;

        private void LineInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            try
            {
                int parse = int.Parse(LineInput.Text);
            }
            catch
            {
                finalResult = DialogResult.None;
                this.Hide();
                return;
            }
            finalResult = DialogResult.Yes;
            this.Hide();
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            RequestedTheme = ((Window.Current.Content as Frame).Content as MainPage).QSetting.Theme;
            finalResult = DialogResult.None;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                int parse = int.Parse(LineInput.Text);
            }
            catch
            {
                finalResult = DialogResult.None;
                this.Hide();
                return;
            }
            finalResult = DialogResult.Yes;
            this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            finalResult = DialogResult.No;
        }
    }
}
