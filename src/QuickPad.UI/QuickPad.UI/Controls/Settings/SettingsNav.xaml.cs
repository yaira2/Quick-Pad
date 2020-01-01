using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class SettingsNav : UserControl
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

        public SettingsNav()
        {
            this.InitializeComponent();

            SettingsFrame.Navigate(typeof(General), new SuppressNavigationTransitionInfo());

            App.Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.ShowSettingsTab):
                    settingNavView.SelectedItem = settingNavView.MenuItems[(int)App.Settings.ShowSettingsTab];
                    ShowTab(App.Settings.ShowSettingsTab);
                    break;
            }
        }

        private Type ShowTab(SettingsTabs settingsTab)
        {
            var pageType = settingsTab switch
            {
                SettingsTabs.General => typeof(General),
                SettingsTabs.Theme => typeof(Theme),
                SettingsTabs.Fonts => typeof(Font),
                SettingsTabs.Advanced => typeof(Advanced),
                SettingsTabs.About => typeof(About),
                _ => null
            };

            if (pageType != null)
            {
                SettingsFrame.Navigate(pageType, new SuppressNavigationTransitionInfo());
            }

            return pageType;
        }

        private void settingNavView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            _ = args.InvokedItemContainer.Tag?.ToString() switch
            {
                "General" => ShowTab(SettingsTabs.General),
                "Theme" => ShowTab(SettingsTabs.Theme),
                "Font" => ShowTab(SettingsTabs.Fonts),
                "Advanced" => ShowTab(SettingsTabs.Advanced),
                "About" => ShowTab(SettingsTabs.About),
                _ => null
            };
        }

        private void settingNavView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            General.IsSelected = true;
            App.Settings.ShowSettings = false;
        }
    }
}
