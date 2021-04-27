using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.Controls
{
    public sealed partial class MenuBar : UserControl
    {
        public event EventHandler NewFileInvoked;

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            NewFileInvoked?.Invoke(this, EventArgs.Empty);
        }
    }
}
