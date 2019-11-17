using QuickPad.Mvvm;
using QuickPad.UI.Common;
using System;
using System.Collections.Generic;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls.Settings
{
    public sealed partial class SettingsNav : UserControl
    {
        public VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
            }
        }

        public SettingsNav()
        {
            this.InitializeComponent();
        }

        private void settingNavView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            Type pageType;
            int tmp = 2;

            switch (args.InvokedItemContainer.Tag.ToString())
            {
                case "General":
                    pageType = typeof(General);
                    break;
                case "Theme":
                    pageType = typeof(Theme);
                    break;
                case "Font":
                    pageType = typeof(Font);
                    break;
                case "Advanced":
                    pageType = typeof(Advanced);
                    break;
                case "About":
                    pageType = typeof(About);
                    break;
                default:
                    pageType = typeof(General);
                    break;
            }

            SettingsFrame.Navigate(pageType, new SuppressNavigationTransitionInfo());
        }
    }
}
