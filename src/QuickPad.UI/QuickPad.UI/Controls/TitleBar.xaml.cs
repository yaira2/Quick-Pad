using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;
using QuickPad.UI.Theme;
using System.ComponentModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

        public WindowsSettingsViewModel Settings => App.Settings;

        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands => App.Commands;

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => DataContext as DocumentViewModel<StorageFile, IRandomAccessStream>;
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
                case nameof(ViewModel.IsDirtyMarker):
                    DirtyMarker.Text = ViewModel.IsDirtyMarker;
                    ApplicationView.GetForCurrentView().Title = ViewModel.IsDirtyMarker + ViewModel.Title + " - Quick Pad";
                    break;

                case nameof(ViewModel.Title):
                    Title.Text = ViewModel.Title;
                    ApplicationView.GetForCurrentView().Title = ViewModel.IsDirtyMarker + ViewModel.Title + " - Quick Pad";
                    break;
            }
        }

        public TitleBar()
        {
            this.InitializeComponent();
            Settings.PropertyChanged += Settings_PropertyChanged;
            Window.Current.SetTitleBar(trickyTitleBar);

            var flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];

            Settings.FlowDirection = flowDirectionSetting == "LTR"
                ? Windows.UI.Xaml.FlowDirection.LeftToRight
                : Windows.UI.Xaml.FlowDirection.RightToLeft;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.CompactOverlay):
                case nameof(Settings.TitleMargin):
                    Bindings.Update();
                    break;

                case nameof(Settings.DefaultTextForegroundBrush):
                    var titleBar = ApplicationView.GetForCurrentView().TitleBar;

                    titleBar.ForegroundColor = Settings.DefaultTextForegroundBrush.Color;
                    titleBar.ButtonForegroundColor = Settings.DefaultTextForegroundBrush.Color;
                    titleBar.ButtonHoverForegroundColor = Settings.DefaultTextForegroundBrush.Color;
                    titleBar.ButtonPressedForegroundColor = Settings.DefaultTextForegroundBrush.Color;

                    break;
            }
        }
    }
}