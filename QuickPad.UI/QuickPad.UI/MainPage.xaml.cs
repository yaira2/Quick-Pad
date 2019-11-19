using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core.Preview;
using QuickPad.UI.Common;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickPad.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDocumentView
    {
        public VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;
        public SettingsViewModel Settings => App.Settings;

        public MainPage()
        {
            App.Controller.AddView(this);
            Initialize?.Invoke(this, Application.Current.Resources[nameof(QuickPadCommands)] as QuickPadCommands);

            this.InitializeComponent();

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            ViewModel.Document = RichEditBox.Document;
            RichEditBox.TextChanged += ViewModel.TextChanged;

            DataContext = ViewModel;

            ViewModel.InitNewDocument();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Bindings.Update();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Title):
                    var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                    appView.Title = ViewModel.Title;
                    break;
            }

            Bindings.Update();
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            e.Handled = true;
            var commands = Application.Current.Resources[nameof(QuickPadCommands)] as QuickPadCommands;
            commands.ExitCommand.Execute(ViewModel);
            App.Settings.ShowSettings = false;
        }

        public DocumentViewModel ViewModel { get; set; }
        public event Action<IDocumentView, QuickPadCommands> Initialize;
    }
}
