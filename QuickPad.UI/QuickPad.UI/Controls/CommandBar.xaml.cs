using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;
using QuickPad.Mvvm.Commands;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class CommandBar
    {
        private string _trySelectFontName;

        public VisualThemeSelector VtSelector { get; } = VisualThemeSelector.Default;

        public SettingsViewModel Settings => App.Settings;
        
        public QuickPadCommands Commands => App.Commands;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
                value.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentFontName):
                    if(ViewModel.CurrentFileType == ".rtf")
                    {
                        ViewModel.Document.Selection.CharacterFormat.Name = ViewModel.CurrentFontName;
                    }
                    else
                    {
                        SetFontName?.Invoke(ViewModel.CurrentFontName);
                    }
                    break;

                case nameof(ViewModel.CurrentFontSize):
                    if (ViewModel.CurrentFileType == ".rtf")
                    {
                        ViewModel.Document.Selection.CharacterFormat.Size = (float)ViewModel.CurrentFontSize;
                    }
                    else
                    {
                        SetFontSize?.Invoke(ViewModel.CurrentFontSize);
                    }
                    break;

                case nameof(ViewModel.CurrentFileType):
                    try
                    {
                        Bindings.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    break;
            }
        }

        public event Action<string> SetFontName;
        public event Action<double> SetFontSize;

        public static DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DocumentViewModel), typeof(CommandBar), null);

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
