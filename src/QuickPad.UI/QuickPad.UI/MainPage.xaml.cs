using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Managers;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Dialogs;
using QuickPad.UI.Helpers;
using QuickPad.UI.Theme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickPad.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : IDocumentView<StorageFile, IRandomAccessStream>
    {
        private DocumentViewModel<StorageFile, IRandomAccessStream> _viewModel;
        private readonly bool _initialized;
        public IServiceProvider Provider { get; }
        public IVisualThemeSelector VtSelector { get; }
        public WindowsSettingsViewModel Settings => App.SettingsViewModel as WindowsSettingsViewModel;
        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands { get; }
        private ILogger<MainPage> Logger { get; }

        private IApplication<StorageFile, IRandomAccessStream> App =>
            ((IApplication<StorageFile, IRandomAccessStream>)Application.Current);

        public MainPage(IServiceProvider provider
            , ILogger<MainPage> logger
            , DocumentViewModel<StorageFile, IRandomAccessStream> viewModel
            , QuickPadCommands<StorageFile, IRandomAccessStream> command
            , IVisualThemeSelector vts)
        {
            Provider = provider;
            VtSelector = vts;
            Logger = logger;
            Commands = command;

            Clipboard.ContentChanged += Clipboard_ContentChanged;

            GotFocus += OnGotFocus;

            Initialize?.Invoke(this, Commands, App);

            this.InitializeComponent();

            var rtfOptions = provider.GetService<RtfDocumentOptions>();

            rtfOptions.Document = RichEditBox.Document;
            rtfOptions.Logger = provider.GetService<ILogger<RtfDocument>>();
            rtfOptions.ViewModel = viewModel;

            var textOptions = provider.GetService<TextDocumentOptions>();

            textOptions.Document = TextBox;
            textOptions.Logger = provider.GetService<ILogger<TextDocument>>();
            textOptions.ViewModel = viewModel;

            CreateNewDocument?.Invoke(this);

            DataContext = ViewModel = viewModel;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var tBar = ApplicationView.GetForCurrentView().TitleBar;
            tBar.ButtonBackgroundColor = Colors.Transparent;
            tBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            Settings.ExitApplication = ExitApp;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            ViewModel.SetScale += ViewModel_SetScale;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.BackRequested += CurrentView_BackRequested;

            commandBar.SetFontName += CommandBarOnSetFontName;
            commandBar.SetFontSize += CommandBarOnSetFontSize;

            if (SystemInformation.TotalLaunchCount == 3)
            {
                var (success, dialog) = provider.GetService<DialogManager>().RequestDialog<AskForReviewDialog>();

                if (!success) return;

                _ = dialog.ShowAsync();
            }

            _initialized = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            TextBox.SelectionFlyout.Opening -= Menu_Opening;
            TextBox.ContextFlyout.Opening -= Menu_Opening;
            RichEditBox.SelectionFlyout.Opening -= Menu_Opening;
            RichEditBox.ContextFlyout.Opening -= Menu_Opening;
        }

        private void Clipboard_ContentChanged(object sender, object e)
        {
            Commands.ContentChangedCommand.Execute(ViewModel);
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
            GainedFocus?.Invoke();
        }

        private void CommandBarOnSetFontSize(float fontSize)
        {
            if (RichEditBox.Document.Selection.FormattedText.CharacterFormat.Size != fontSize)
            {
                RichEditBox.Document.Selection.FormattedText.CharacterFormat.Size = fontSize;
            }
        }

        private void CommandBarOnSetFontName(string fontFamilyName)
        {
            if (RichEditBox.Document.Selection.FormattedText.CharacterFormat.Name != fontFamilyName)
            {
                RichEditBox.Document.Selection.FormattedText.CharacterFormat.Name = fontFamilyName;
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var newMode = Settings.DefaultMode == DisplayModes.LaunchFocusMode.ToString()
                ? DisplayModes.LaunchClassicMode.ToString()
                : Settings.DefaultMode;

            SetOverlayMode(newMode)
                .ContinueWith(_ => { Settings.CurrentMode = newMode; });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupFocusMode(Settings.FocusMode);

            await SetOverlayMode(Settings.CurrentMode);

            Settings.Dispatch(() => Bindings.Update());

            if (Settings.DefaultMode == "Compact Overlay")
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
                {
                    //set the current mode to be the launch mode
                    Settings.CurrentMode = Settings.DefaultMode;
                }
            }

            if (Settings.DefaultMode == "LaunchFocusMode")
            {
                //set return mode in case the user wants to leave focus mode
                Settings.ReturnToMode = nameof(DisplayModes.LaunchClassicMode);

                //set the current mode to be the launch mode
                Settings.CurrentMode = Settings.DefaultMode;
            }

            Settings.NotDeferred = true;
            Settings.Status("Ready", TimeSpan.FromSeconds(10), Verbosity.Release);

            if (ViewModel.CurrentFileType == ".rtf")
            {
                RichEditBox.Focus(FocusState.Programmatic);
            }
            else
            {
                TextBox.Focus(FocusState.Programmatic);
            }

            TextBox.SelectionFlyout.Opening += Menu_Opening;
            TextBox.ContextFlyout.Opening += Menu_Opening;
            RichEditBox.SelectionFlyout.Opening += RMenu_Opening;
            RichEditBox.ContextFlyout.Opening += RMenu_Opening;
        }

        public event Func<DocumentViewModel<StorageFile, IRandomAccessStream>, StorageFile, Task> LoadFromFile;

        public event Action<IDocumentView<StorageFile, IRandomAccessStream>> SaveToFile;

        public event Action<IDocumentView<StorageFile, IRandomAccessStream>> CreateNewDocument;

        public event Action GainedFocus;

        private async Task SetOverlayMode(string mode)
        {
            if (mode == DisplayModes.LaunchCompactOverlay.ToString())
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
                {
                    //set a return mode so the user can get out of compact overlay mode
                    Settings.ReturnToMode = nameof(DisplayModes.LaunchClassicMode);
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

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
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
            if (App.Services.GetService<DialogManager>().CurrentDialogView != null)
            {
                e.Handled = true;
                Logger.LogCritical("Already a dialog open.");
                return;
            }

            ViewModel.Deferral = e.GetDeferral();

            if (ExitApplication == null) ((Windows.Foundation.Deferral)ViewModel.Deferral).Complete();
            else
            {
                e.Handled = !(await ExitApplication(ViewModel));

                if (!e.Handled) return;

                try
                {
                    ((Windows.Foundation.Deferral)ViewModel.Deferral)?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    Logger.LogDebug("Handled Deferral already disposed.");
                }
            }
        }

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel == value) return;

                if (_viewModel != null)
                {
                    _viewModel.RedoRequested -= ViewModelOnRedoRequested;
                    _viewModel.UndoRequested -= ViewModelOnUndoRequested;
                    _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                    _viewModel.SetSelection -= ViewModelOnSetSelection;
                    _viewModel.GetPosition -= ViewModelOnGetPosition;
                    _viewModel.SetSelectedText -= ViewModelOnSetSelectedText;
                    _viewModel.ClearUndoRedo -= ViewModelOnClearUndoRedo;
                    _viewModel.Focus -= ViewModelOnFocus;
                    _viewModel.SaveDocument -= ViewModelOnSaveDocument;

                    RichEditBox.TextChanged -= _viewModel.TextChanged;
                    TextBox.TextChanged -= _viewModel.TextChanged;
                }

                _viewModel = value;

                _viewModel.RedoRequested += ViewModelOnRedoRequested;
                _viewModel.UndoRequested += ViewModelOnUndoRequested;
                _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
                _viewModel.SetSelection += ViewModelOnSetSelection;
                _viewModel.GetPosition += ViewModelOnGetPosition;
                _viewModel.SetSelectedText += ViewModelOnSetSelectedText;
                _viewModel.ClearUndoRedo += ViewModelOnClearUndoRedo;
                _viewModel.Focus += ViewModelOnFocus;
                _viewModel.SaveDocument += ViewModelOnSaveDocument;

                _viewModel.NewDocumentInitialized += ViewModelOnNewDocumentInitialized;

                if (_initialized) return;

                if (ViewModel.Document == null)
                {
                    ViewModel.InitNewDocument();
                    Initialize?.Invoke(this, Commands, App);
                }

                RichEditBox.TextChanged += _viewModel.TextChanged;
                TextBox.TextChanged += _viewModel.TextChanged;
            }
        }

        private void ViewModelOnSaveDocument(DocumentViewModel<StorageFile, IRandomAccessStream> obj)
        {
            App.TryEnqueue(() => SaveToFile?.Invoke(this));
        }

        private void ViewModelOnNewDocumentInitialized(DocumentModel<StorageFile, IRandomAccessStream> obj)
        {
            switch (obj)
            {
                case RtfDocument rtfDocument:
                    RichEditBox.SelectionChanging += (sender, args) => rtfDocument.NotifyOnSelectionChange();
                    break;
            };
        }

        public DocumentModel<StorageFile, IRandomAccessStream> ViewModelDocument => _viewModel.Document;

        private void ViewModelOnFocus()
        {
            if (ViewModel.IsRtf)
            {
                RichEditBox.Focus(FocusState.Programmatic);
            }
            else
            {
                TextBox.Focus(FocusState.Programmatic);
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
            ViewModel.Document.SetSelectedText(text);
        }

        private (int start, int length) ViewModelOnGetPosition()
        {
            return _viewModel.Document?.GetSelectionBounds() ?? (0, 0);
        }

        private void ViewModelOnSetSelection(int start, int length, bool reindex = true)
        {
            try
            {
                App.TryEnqueue(() =>
                {
                    _viewModel.Document.SetSelectionBound(start, length);

                    if (reindex) Reindex();
                });
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
                case nameof(DocumentViewModel<StorageFile, IRandomAccessStream>.File):
                    Reindex();
                    break;
            }
        }

        private void ViewModelOnUndoRequested(DocumentViewModel<StorageFile, IRandomAccessStream> obj)
        {
            if (ViewModel.IsRtf)
            {
                if (!RichEditBox.Document.CanUndo()) return;
                RichEditBox.Document.Undo();
                Reindex();
            }
            else
            {
                if (!TextBox.CanUndo) return;
                TextBox.Undo();
                Reindex();
            }
        }

        private void ViewModelOnRedoRequested(DocumentViewModel<StorageFile, IRandomAccessStream> obj)
        {
            if (ViewModel.IsRtf)
            {
                if (!RichEditBox.Document.CanRedo()) return;
                RichEditBox.Document.Redo();
                Reindex();
            }
            else
            {
                if (!TextBox.CanRedo) return;
                TextBox.Redo();
                Reindex();
            }
        }

        public event Action<IDocumentView<StorageFile
            , IRandomAccessStream>, IQuickPadCommands<StorageFile, IRandomAccessStream>
            , IApplication<StorageFile, IRandomAccessStream>> Initialize;

        public event Func<DocumentViewModel<StorageFile, IRandomAccessStream>, Task<bool>> ExitApplication;

        private async void MainPage_OnKeyUp(object sender, KeyRoutedEventArgs args)
        {
            var controlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var menuDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down);
            var shiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            var leftWindowsDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down);
            var rightWindowsDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.RightWindows).HasFlag(CoreVirtualKeyStates.Down);

            //ctrl + +
            //zoom in
            if (controlDown & args.Key == (VirtualKey)187)
            {
                Commands.ZoomInCommand.Execute(ViewModel);
            }

            //ctrl + -
            //zoom out
            if (controlDown & args.Key == (VirtualKey)189)
            {
                Commands.ZoomOutCommand.Execute(ViewModel);
            }

            //ctrl + 0
            //reset zoom
            if (controlDown & args.Key == (VirtualKey)48)
            {
                Commands.ResetZoomCommand.Execute(ViewModel);
            }

            //alt + +
            //superscript
            if (menuDown & args.Key == (VirtualKey)187)
            {
                Commands.SuperscriptCommand.Execute(ViewModel);
            }

            //alt + -
            //subscript
            if (menuDown & args.Key == (VirtualKey)189)
            {
                Commands.SubscriptCommand.Execute(ViewModel);
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

            Settings.Status($"{Settings.CurrentModeText} Enabled.", TimeSpan.FromSeconds(5), Verbosity.Debug);
        }

        private void RichEditBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
        }

        private void TextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            ViewModelDocument.NotifyOnSelectionChange();
            GetPosition(TextBox.SelectionStart + TextBox.SelectionLength);
        }

        private (int start, int length) _lastSelectionRange = default;

        private List<int> LineIndices => ViewModel.Document.LineIndices;
        public StorageFile FileToLoad { get; set; }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsRtf)
            {
                TextBox.SelectionChanged -= TextBox_OnSelectionChanged;

                ViewModel.SetText(TextBox.Text, true);

                Reindex();

                TextBox_OnSelectionChanged(sender, e);

                TextBox.SelectionChanged += TextBox_OnSelectionChanged;

                Provider.GetService<IQuickPadCommands<StorageFile, IRandomAccessStream>>().RefreshStates(ViewModel);

                if (Settings.AutoSave)
                {
                    ViewModel.ResetTimer();
                }
            }
        }

        private void GetPosition(int position)
        {
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

        public void Reindex()
        {
            ViewModel.Document.Reindex();

            var index = -1;
            var text = ViewModel.Text.Replace(Environment.NewLine, "\r");
            while ((index = text.IndexOf('\r', index + 1)) > -1)
            {
                index++;
                LineIndices.Add(index);

                if (index + 1 >= text.Length) break;
            }

            GetPosition(ViewModel.IsRtf
                ? RichEditBox.Document.Selection.StartPosition + RichEditBox.Document.Selection.Length
                : TextBox.SelectionStart + TextBox.SelectionLength);
        }

        private void RichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsRtf)
            {
                Reindex();

                if (Settings.AutoSave)
                {
                    ViewModel.ResetTimer();
                }

                Provider.GetService<IQuickPadCommands<StorageFile, IRandomAccessStream>>().RefreshStates(ViewModel);
            }
        }

        private void Text_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            //add tab space
            if (e.Key != VirtualKey.Tab || Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift)
                    .HasFlag(CoreVirtualKeyStates.Down)) return;

            ViewModel.AddTab();

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

        private void Menu_Opening(object sender, object e)
        {
            if (!(sender is TextCommandBarFlyout myFlyout) || myFlyout.Target != TextBox) return;
            AddSearchMenuItems(myFlyout.PrimaryCommands);
        }

        private void RMenu_Opening(object sender, object e)
        {
            if (!(sender is TextCommandBarFlyout myFlyout) || myFlyout.Target != RichEditBox) return;
            AddSearchMenuItems(myFlyout.PrimaryCommands);
        }

        private void AddSearchMenuItems(IObservableVector<ICommandBarElement> primaryCommands)
        {
            if (!primaryCommands.Any(b => b is AppBarButton button && button.Name == "Bing"))
            {
                var iconBing = new BitmapIcon { UriSource = new Uri("ms-appx:///Assets/BingIcon.png") };

                var searchCommandBarBing = new AppBarButton
                {
                    Name = "Bing",
                    Icon = iconBing,
                    Label = "Search with Bing",
                    Command = Commands.BingCommand,
                    CommandParameter = Settings
                };
                primaryCommands.Add(searchCommandBarBing);
            }

            if (Settings.EnableGoogleSearch != true ||
                primaryCommands.Any(b => b is AppBarButton button && button.Name == "Google")) return;

            var iconGoogle = new BitmapIcon { UriSource = new Uri("ms-appx:///Assets/GoogleIcon.png") };
            var searchCommandBarGoogle = new AppBarButton
            {
                Name = "Google",
                Icon = iconGoogle,
                Label = "Search with Google",
                Command = Commands.GoogleCommand,
                CommandParameter = Settings
            };
            primaryCommands.Add(searchCommandBarGoogle);
        }

        private void RichEditBox_OnSelectionChanging(RichEditBox sender, RichEditBoxSelectionChangingEventArgs args)
        {
            try
            {
                GetPosition(args.SelectionStart + args.SelectionLength);

                if (_lastSelectionRange.start != args.SelectionStart ||
                    _lastSelectionRange.length != args.SelectionLength)
                {
                    _lastSelectionRange = (args.SelectionStart, args.SelectionLength);
                }
            }
            catch (Exception e)
            {
                Logger.LogCritical(new EventId(), "RichEditBox_OnSelectionChanging caught exception.", e);
            }
        }
    }
}