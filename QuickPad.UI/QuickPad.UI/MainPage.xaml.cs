using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.ApplicationModel.Store;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.WindowManagement;
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
        private QuickPadCommands Commands => App.Current.Resources[nameof(QuickPadCommands)] as QuickPadCommands;

        public MainPage()
        {
            App.Controller.AddView(this);
            Initialize?.Invoke(this, Commands);

            this.InitializeComponent();

            Loaded += OnLoaded;

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            ViewModel.Document = RichEditBox.Document;
            RichEditBox.TextChanged += ViewModel.TextChanged;

            DataContext = ViewModel;

            Task.Run(ViewModel.InitNewDocument).Wait();

            ViewModel.ExitApplication = ExitApplication;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += CurrentView_BackRequested;

            commandBar.SetFontName += CommandBarOnSetFontName;
            commandBar.SetFontSize += CommandBarOnSetFontSize;
        }

        private void CommandBarOnSetFontSize(double fontSize)
        {
            RichEditBox.FontSize = fontSize;
        }

        private void CommandBarOnSetFontName(string fontFamilyName)
        {
            RichEditBox.FontFamily = new FontFamily(fontFamilyName);
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Settings.CurrentMode = Settings.PreviousMode;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!Settings.IsFullScreenMode && Settings.NotDeferred)
            {
                var ttv = this.TransformToVisual(Window.Current.Content);
                var windowBounds =
                    ttv.TransformBounds(new Rect(Settings.Left, Settings.Top, Settings.Width, Settings.Height));

                if (ApplicationView.GetForCurrentView()
                    .TryResizeView(new Size(windowBounds.Width, windowBounds.Height))) return;

                var dialog = new MessageDialog($"Could not set size ({Settings.Width}, {Settings.Height}).", "UWP Refused Setting Size.");
                await dialog.ShowAsync();
            }
            else if(Settings.IsFullScreenMode)
            {
                if (ApplicationView.GetForCurrentView().TryEnterFullScreenMode()) return;

                var dialog = new MessageDialog("Could not enter full screen.", "UWP Refused Full Screen.");
                await dialog.ShowAsync();
            }

            Settings.NotDeferred = true;
        }

        private void ExitApplication()
        {
            var ttv = this.TransformToVisual(Window.Current.Content);
            var windowCoords = ttv.TransformPoint(new Point());
            var screenCoordsX = windowCoords.X + ApplicationView.GetForCurrentView().VisibleBounds.Left;
            var screenCoordsY = windowCoords.Y + ApplicationView.GetForCurrentView().VisibleBounds.Top;

            var windowBounds = ttv.TransformBounds(new Rect());

            Settings.Top = screenCoordsY;
            Settings.Left = screenCoordsX;
            Settings.Width = ApplicationView.GetForCurrentView().VisibleBounds.Width;
            Settings.Height = ApplicationView.GetForCurrentView().VisibleBounds.Height;

            Settings.IsFullScreenMode = ApplicationView.GetForCurrentView().IsFullScreenMode;

            CoreApplication.Exit();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Bindings.Update();
            var currentView = SystemNavigationManager.GetForCurrentView();
            switch (e.PropertyName)
            {
                case nameof(Settings.FocusMode) when Settings.FocusMode:
                    currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    break;

                case nameof(Settings.FocusMode) when !Settings.FocusMode:
                    currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    break;

            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Title):
                    var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                    appView.Title = ViewModel.Title;
                    break;

                case nameof(ViewModel.Text):
                    Commands.NotifyChanged(ViewModel, Settings);
                    break;
            }

            Bindings.Update();
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            ViewModel.Deferral = e.GetDeferral();

            Commands.ExitCommand.Execute(ViewModel);
        }

        public DocumentViewModel ViewModel { get; set; }
        public event Action<IDocumentView, QuickPadCommands> Initialize;
    }
}
