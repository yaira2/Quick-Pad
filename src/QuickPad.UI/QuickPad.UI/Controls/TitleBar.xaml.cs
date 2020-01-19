<<<<<<< HEAD
﻿using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.UI.Common.Theme;


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
            }
        }

        public TitleBar()
        {
            this.InitializeComponent();
            Settings.PropertyChanged += Settings_PropertyChanged;
            Window.Current.SetTitleBar(trickyTitleBar);

            var flowDirectionSetting = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            if (flowDirectionSetting == "LTR")
            {
                Settings.FlowDirection = Windows.UI.Xaml.FlowDirection.LeftToRight;
            }
            else
            {
                Settings.FlowDirection = Windows.UI.Xaml.FlowDirection.RightToLeft;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
=======
﻿using System.ComponentModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.UI.Common.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

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

        public TitleBar()
        {
            this.InitializeComponent();
            Settings.PropertyChanged += Settings_PropertyChanged;
            Window.Current.SetTitleBar(trickyTitleBar);

            var flowDirectionSetting = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            if (flowDirectionSetting == "LTR")
            {
                Settings.FlowDirection = Windows.UI.Xaml.FlowDirection.LeftToRight;
            }
            else
            {
                Settings.FlowDirection = Windows.UI.Xaml.FlowDirection.RightToLeft;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
>>>>>>> master
