using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
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
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.WindowManagement;
using Microsoft.Extensions.Logging;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Common.Dialogs;
using QuickPad.UI.Common.Theme;
using QuickPad.UI.Controls;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickPad.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDocumentView
    {
        private DocumentViewModel _viewModel;
        private readonly bool _initialized;
        public IVisualThemeSelector VTSelector { get; }
        public SettingsViewModel Settings => App.Settings;
        public QuickPadCommands Commands { get; }
        private ILogger<MainPage> Logger { get; }

        public MainPage(ILogger<MainPage> logger, DocumentViewModel viewModel
            , QuickPadCommands command, IVisualThemeSelector vts)
        {
            VTSelector = vts;
            Logger = logger;
            Commands = command;

            GotFocus += OnGotFocus;

            App.Controller.AddView(this);

            Initialize?.Invoke(this, Commands);

            this.InitializeComponent();
            _initialized = true;

            DataContext = ViewModel = viewModel;

            Loaded += OnLoaded;

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var tBar = ApplicationView.GetForCurrentView().TitleBar;
            tBar.ButtonBackgroundColor = Colors.Transparent;
            tBar.ButtonInactiveBackgroundColor = Colors.Transparent;


            ViewModel.ExitApplication = ExitApp;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            ViewModel.SetScale += ViewModel_SetScale;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += CurrentView_BackRequested;

            commandBar.SetFontName += CommandBarOnSetFontName;
            commandBar.SetFontSize += CommandBarOnSetFontSize;
        }

        public void ViewModel_SetScale(float scale)
        {
            TextScrollViewer.ChangeView(0.0, 0.0, ViewModel.ScaleValue);
        }

        private void TextScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ViewModel.ScaleValue = TextScrollViewer.ZoomFactor;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            GainedFocus?.Invoke(e);
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
            var newMode = Settings.DefaultMode == DisplayModes.LaunchFocusMode.ToString()
                ? DisplayModes.LaunchClassicMode.ToString()
                : Settings.DefaultMode;

            SetOverlayMode(newMode)
                .ContinueWith(_ => { Settings.CurrentMode = newMode; });
        }

        public StorageFile FileToLoad { get; set; }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupFocusMode(Settings.FocusMode);

            await SetOverlayMode(Settings.CurrentMode);
            await ViewModel.InitNewDocument();

            Mvvm.ViewModels.ViewModel.Dispatch(() => Bindings.Update());

            if (Settings.DefaultMode == "Compact Overlay")
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
                {
                    Settings.CurrentMode = "Compact Overlay";
                }
            }

            Settings.NotDeferred = true;
            Settings.Status("Ready", TimeSpan.FromSeconds(10), SettingsViewModel.Verbosity.Release);

            if (FileToLoad != null && LoadFromFile != null)
            {
                await LoadFromFile(ViewModel, FileToLoad);
                FileToLoad = null;
            }

            if (ViewModel.CurrentFileType == ".rtf")
            {
                RichEditBox.Focus(FocusState.Programmatic);
            }
            else
            {
                TextBox.Focus(FocusState.Programmatic);
            }
        }

        public event Func<DocumentViewModel, StorageFile, Task> LoadFromFile;
        public event Action<RoutedEventArgs> GainedFocus;

        private async Task SetOverlayMode(string mode)
        {
            if (mode == DisplayModes.LaunchCompactOverlay.ToString())
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
                {
                    Settings.CurrentMode = mode;
                }
            }
            else
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default))
                {
                    Settings.CurrentMode = Settings.ReturnToMode;
                }
            }
        }

        private void ExitApp()
        {
            CoreApplication.Exit();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Bindings.Update();
            switch (e.PropertyName)
            {
                case nameof(Settings.CurrentMode):
                    SetupFocusMode(Settings.FocusMode);
                    break;
            }
        }

        private void SetupFocusMode(bool enabled)
        {
            var currentView = SystemNavigationManager.GetForCurrentView();
            if (enabled)
            {
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                var di = DisplayInformation.GetForCurrentView();
                Settings.BackButtonWidth = 30 * ((double)di.ResolutionScale / 100.0);
            }
            else
            {
                currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Title):
                    var appView = ApplicationView.GetForCurrentView();
                    appView.Title = ViewModel.Title;
                    break;

                case nameof(ViewModel.Text):
                    Commands.NotifyChanged(ViewModel, Settings);
                    break;
            }

            try
            {
                if (!e.PropertyName.Equals(nameof(ViewModel.Text)))
                {
                    Bindings.Update();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error binding objects.");
            }

        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            ViewModel.Deferral = e.GetDeferral();

            if (ExitApplication == null) ViewModel.Deferral.Complete();
            else
            {
                e.Handled = !(await ExitApplication(ViewModel));

                if (!e.Handled) return;

                try
                {
                    ViewModel.Deferral?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    Logger.LogDebug("Handled Deferral already disposed.");
                }
            }
        }

        public DocumentViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    if (_viewModel != null)
                    {
                        RichEditBox.TextChanged -= _viewModel.TextChanged;
                        TextBox.TextChanged -= _viewModel.TextChanged;

                        _viewModel.RedoRequested -= ViewModelOnRedoRequested;
                        _viewModel.UndoRequested -= ViewModelOnUndoRequested;
                        _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                        _viewModel.SetSelection -= ViewModelOnSetSelection;
                        _viewModel.GetPosition -= ViewModelOnGetPosition;
                        _viewModel.SetSelectedText -= ViewModelOnSetSelectedText;
                        _viewModel.ClearUndoRedo -= ViewModelOnClearUndoRedo;
                    }

                    _viewModel = value;

                    _viewModel.RedoRequested += ViewModelOnRedoRequested;
                    _viewModel.UndoRequested += ViewModelOnUndoRequested;
                    _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
                    _viewModel.SetSelection += ViewModelOnSetSelection;
                    _viewModel.GetPosition += ViewModelOnGetPosition;
                    _viewModel.SetSelectedText += ViewModelOnSetSelectedText;
                    _viewModel.ClearUndoRedo += ViewModelOnClearUndoRedo;

                    if (!_initialized) return;

                    ViewModel.Document = RichEditBox.Document;
                    RichEditBox.TextChanged += _viewModel.TextChanged;
                    TextBox.TextChanged += _viewModel.TextChanged;
                }
            }
        }

        private void ViewModelOnClearUndoRedo()
        {
            if (TextBox.CanUndo || TextBox.CanRedo)
            {
                TextBox.ClearUndoRedoHistory();
            }
        }

        private void ViewModelOnSetSelectedText(string text)
        {
            if (_viewModel.IsRtf)
            {
                _viewModel.Document.Selection.SetText(_viewModel.SetOption, text);
            }
            else
            {
                TextBox.SelectedText = text;
                ViewModel.Text = TextBox.Text;
            }
        }

        private (int start, int length) ViewModelOnGetPosition()
        {
            return _viewModel.IsRtf
                ? (_viewModel.Document.Selection.StartPosition, _viewModel.Document.Selection.Length)
                : (TextBox.SelectionStart, TextBox.SelectionLength);
        }

        private async Task ViewModelOnSetSelection(int start, int length)
        {
            try
            {
                if (_viewModel.IsRtf)
                {
                    _viewModel.Document.Selection.StartPosition = start;
                    _viewModel.Document.Selection.EndPosition = start + length;
                    await FocusManager.TryFocusAsync(RichEditBox, FocusState.Programmatic);
                }
                else
                {
                    TextBox.SelectionStart = start;
                    TextBox.SelectionLength = length;
                    await FocusManager.TryFocusAsync(TextBox, FocusState.Programmatic);
                }

                Reindex();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Invalid position for selection. (start: {start}, length: {length}).");
            }
        }


        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DocumentViewModel.File):
                    Reindex();
                    break;
            }
        }

        private void ViewModelOnUndoRequested(DocumentViewModel obj)
        {
            if (TextBox.CanUndo)
            {
                TextBox.Undo();
                Reindex();
            }
        }

        private void ViewModelOnRedoRequested(DocumentViewModel obj)
        {
            if (TextBox.CanRedo)
            {
                TextBox.Redo();
                Reindex();
            }
        }

        public event Action<IDocumentView, QuickPadCommands> Initialize;
        public event Func<DocumentViewModel, Task<bool>> ExitApplication;

        private async void MainPage_OnKeyUp(object sender, KeyRoutedEventArgs args)
        {
            var controlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) .HasFlag(CoreVirtualKeyStates.Down);
            var menuDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down);
            var shiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) .HasFlag(CoreVirtualKeyStates.Down);
            var leftWindowsDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftWindows) .HasFlag(CoreVirtualKeyStates.Down);
            var rightWindowsDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.RightWindows) .HasFlag(CoreVirtualKeyStates.Down);

            if (controlDown & args.Key == (VirtualKey)187)
            {
                Commands.ZoomIn.Execute(ViewModel);
            }

            if (controlDown & args.Key == (VirtualKey)189)
            {
                Commands.ZoomOut.Execute(ViewModel);
            }

            if (controlDown & args.Key == (VirtualKey)48)
            {
                Commands.ResetZoom.Execute(ViewModel);
            }

            var option = (c: controlDown, s: shiftDown, m: menuDown, l: leftWindowsDown, r: rightWindowsDown, k: args.Key);

            var newMode = option switch
            {
                (true, true, false, false, false, VirtualKey.Number1) => DisplayModes.LaunchClassicMode.ToString(),
                (true, true, false, false, false, VirtualKey.Number2) => DisplayModes.LaunchDefaultMode.ToString(),
                (true, true, false, false, false, VirtualKey.Number3) => DisplayModes.LaunchFocusMode.ToString(),
                (false, false, true, false, false, VirtualKey.Up) => DisplayModes.LaunchCompactOverlay.ToString(),
                (false, false, true, false, false, VirtualKey.Down) => Settings.CurrentMode == DisplayModes.LaunchCompactOverlay.ToString() ? Settings.DefaultMode : Settings.CurrentMode,
                (true, false, false, false, false, VirtualKey.F12) => DisplayModes.LaunchNinjaMode.ToString(),
                _ => Settings.CurrentMode
            };

            if (Settings.CurrentMode == newMode) return;

            SetupFocusMode(newMode == DisplayModes.LaunchFocusMode.ToString());

            await SetOverlayMode(newMode);

            Settings.CurrentMode = newMode;

            Settings.Status($"{Settings.CurrentModeText} Enabled.", TimeSpan.FromSeconds(5), SettingsViewModel.Verbosity.Debug);
        }

        private void RichEditBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        private void TextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            GetPosition();
        }

        private List<int> LineIndices { get; } = new List<int>();

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsRtf)
            {
                TextBox.SelectionChanged -= TextBox_OnSelectionChanged;

                ViewModel.Text = TextBox.Text;

                Reindex();

                TextBox_OnSelectionChanged(sender, e);

                TextBox.SelectionChanged += TextBox_OnSelectionChanged;

                if (Settings.AutoSave)
                {
                    ViewModel.ResetTimer();
                }
            }
        }

        private void GetPosition()
        {
            var position = TextBox.SelectionStart + TextBox.SelectionLength;

            var target = position;

            var lines = LineIndices.Where(i => i < target).ToList();

            if (lines.Count != 0)
            {
                var lastMarker = lines.Max(i => i);

                ViewModel.CurrentColumn = position - lastMarker;
                ViewModel.CurrentLine = LineIndices.IndexOf(lastMarker) + 2;
            }
            else
            {
                ViewModel.CurrentLine = 1;
                ViewModel.CurrentColumn = position + 1;
            }
        }

        private void TextBox_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            TextBox.SelectionChanged -= TextBox_OnSelectionChanged;
        }

        private void Reindex()
        {
            LineIndices.Clear();

            var index = -1;
            var text = ViewModel.Text;
            while ((index = text.IndexOf('\r', index + 1)) > -1)
            {
                LineIndices.Add(index);
            }

            GetPosition();
        }

        private void RichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if(ViewModel.IsRtf)
            {
                Reindex();

                if (Settings.AutoSave)
                {
                    ViewModel.ResetTimer();
                }
            }
        }

        private async void TextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Tab || Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift)
                    .HasFlag(CoreVirtualKeyStates.Down)) return;

            await ViewModel.AddTab();

            e.Handled = true;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy | DataPackageOperation.Link | DataPackageOperation.Move;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count <= 0) return;

            var storageFile = items[0] as StorageFile;
            LoadFromFile?.Invoke(ViewModel, storageFile);
        }
    }
}
