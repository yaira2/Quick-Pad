using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class CommandBar
    {
        private string _trySelectFontName;

        public VisualThemeSelector VtSelector { get; } = VisualThemeSelector.Default;

        public SettingsViewModel Settings => App.Settings;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
            }
        }

        public static DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DocumentViewModel), typeof(CommandBar), null);

        private string _trySelectSize;

        public CommandBar()
        {
            this.InitializeComponent();
        }

        private void OpenFontFlyout(object sender, object e)
        {
            _trySelectFontName = "";
            FontListSelection.Focus(FocusState.Programmatic);
            FontListSelection.ScrollIntoView(FontListSelection.SelectedItem);
        }

        private void OpenFontSizeFlyout(object sender, object e)
        {
            _trySelectSize = "";
            FontSizeListSelection.Focus(FocusState.Programmatic);
            FontSizeListSelection.ScrollIntoView(FontSizeListSelection.SelectedItem);
        }

        private void TryToFindFont(UIElement sender, CharacterReceivedRoutedEventArgs args)
        {
            _trySelectFontName += args.Character;
            var trySelect = Settings.AllFonts.FirstOrDefault(i => i.StartsWith(_trySelectFontName, StringComparison.InvariantCultureIgnoreCase));

            if (trySelect is null)
                return;

            FontListSelection.ScrollIntoView(trySelect, ScrollIntoViewAlignment.Leading);
            FontListSelection.SelectedItem = trySelect;
        }

    }
}
