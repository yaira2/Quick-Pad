using QuickPad.UI.Common.Theme;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.UI.Common.Dialogs
{
    public sealed partial class GoToLine : IGoToLineView
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;
        public QuickPadCommands Commands { get; }

        private DocumentViewModel _viewModel;
        public DocumentViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                DataContext = _viewModel = value;
            }
        }

        public GoToLine(QuickPadCommands commands)
        {
            Commands = commands;
            this.InitializeComponent();
        }

        private void CmdClose_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Hide();
        }

        private void GotoTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //GotoTextBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            GotoTextBox.SelectAll();
        }

        private void GotoTextBox_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (GotoTextBox.Text.Length == 0) return;

            if (System.Int32.TryParse(GotoTextBox.Text, out var line) &&
                line > 0 &&
                line <= ViewModel.LineIndices.Count)
            {
                ViewModel.LineToGoTo = line;
            }
            else
            {
                var old = GotoTextBox.Text.Substring(0, GotoTextBox.Text.Length - 1);
                GotoTextBox.Text = ViewModel.LineToGoTo.ToString();

                if (!old.StartsWith(GotoTextBox.Text))
                {
                    GotoTextBox.SelectAll();
                }
                else
                {
                    GotoTextBox.Select(old.Length, 0);
                }
            }
        }
    }
}
