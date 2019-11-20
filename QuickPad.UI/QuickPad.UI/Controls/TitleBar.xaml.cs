using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

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

        public TitleBar()
        {
            this.InitializeComponent();
            Settings.PropertyChanged += Settings_PropertyChanged;
            Window.Current.SetTitleBar(trickyTitleBar);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.TitleMargin):
                    Bindings.Update();
                    break;
            }
        }
    }
}
