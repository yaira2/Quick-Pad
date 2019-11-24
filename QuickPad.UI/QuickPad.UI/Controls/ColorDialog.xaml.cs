using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Common;
using QuickPad.Mvvm.Commands;
using QuickPad.UI.Common.Theme;
using System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class ColorDialog : UserControl
    {
        public VisualThemeSelector VisualThemeSelector => VisualThemeSelector.Current;

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

        public ColorDialog()
        {
            this.InitializeComponent();
        }
    }
}
