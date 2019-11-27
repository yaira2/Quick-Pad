using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;
using QuickPad.Mvvm.Commands;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Common.Theme;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class MenuBar : UserControl
    {
        public VisualThemeSelector VisualThemeSelector => VisualThemeSelector.Current;

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

        public MenuBar()
        {
            this.InitializeComponent();

            Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.CurrentMode):
                    Bindings.Update();
                    break;
            }
        }
    }
}
