using QuickPad.UI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class SettingsNav : UserControl
    {
        public IVisualThemeSelector VTSelector => VisualThemeSelector.Current;

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

        private Type ShowTab(SettingsViewModel.SettingsTabs settingsTab)
        {
            var pageType = settingsTab switch
            {
                SettingsViewModel.SettingsTabs.General => typeof(General),
                SettingsViewModel.SettingsTabs.Theme => typeof(Theme),
                SettingsViewModel.SettingsTabs.Fonts => typeof(Font),
                SettingsViewModel.SettingsTabs.Advanced => typeof(Advanced),
                SettingsViewModel.SettingsTabs.About => typeof(About),
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
                "General" => ShowTab(SettingsViewModel.SettingsTabs.General),
                "Theme" => ShowTab(SettingsViewModel.SettingsTabs.Theme),
                "Font" => ShowTab(SettingsViewModel.SettingsTabs.Fonts),
                "Advanced" => ShowTab(SettingsViewModel.SettingsTabs.Advanced),
                "About" => ShowTab(SettingsViewModel.SettingsTabs.About),
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
