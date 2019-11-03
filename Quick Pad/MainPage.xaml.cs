using Microsoft.AppCenter.Analytics;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace QuickPad
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Notification overhead, no need to write it thousands times on set { }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Set property and also alert the UI if the value is changed
        /// </summary>
        /// <param name="value">New value</param>
        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Alert the UI there is a change in this property and need update
        /// </summary>
        /// <param name="name"></param>
        public void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private const char RichEditBoxDefaultLineEnding = '\r';
        private string[] _contentLinesCache;
        private bool _isLineCachePendingUpdate = true;
        private string _content = string.Empty;

        public ResourceLoader textResource { get; } = ResourceLoader.GetForCurrentView(); //Use to get a text resource from Strings/en-US

        public QuickPad.VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

        public Setting QSetting => App.QSetting;

        public QuickPad.Dialog.SaveChange WantToSave = new QuickPad.Dialog.SaveChange();

        public Dialog.GoTo GoToDialog = new Dialog.GoTo();
        public MainPage()
        {
            InitializeComponent();
            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            Window.Current.SetTitleBar(trickyTitleBar);

            //Subscribe to events
            VisualThemeSelector.ThemeChanged += UpdateUIAccordingToNewTheme;
            QSetting.afterFontSizeChanged += UpdateText1FontSize;
            UpdateText1FontSize(QSetting.DefaultFontSize);
            QSetting.afterAutoSaveChanged += UpdateAutoSave;
            QSetting.afterAutoSaveIntervalChanged += UpdateAutoSaveInterval;
            QSetting.afterFontColorChanged += UpdateFontColor;
            //
            LoadSettings();
            AllFonts = new ObservableCollection<FontFamilyItem>(CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font).Select(font => new FontFamilyItem(font)));
            //

            Clipboard.ContentChanged += ClipboardStatusUpdate;
            ClipboardStatusUpdate(null, null);

            //check if focus is on app or off the app
            Window.Current.CoreWindow.Activated += (sender, args) =>
            {
                if (args.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                {
                    if (CommandBar2.Visibility == Visibility.Visible)
                    {
                        CommandBar2.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                    else if (BackButtonHolder.Visibility == Visibility.Visible)
                    {
                        BackButtonHolder.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                    else if (CommandBarClassic.Visibility == Visibility.Visible)
                    {
                        CommandBarClassic.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                    else if (CloseCompactOverlay.Visibility == Visibility.Visible)
                    {
                        CloseCompactOverlay.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                }
            };

            Window.Current.CoreWindow.KeyDown += (sender, args) =>
            {
                if (CompactOverlaySwitch)//Not allow to switch on focus mode
                    return;
                //CTRL
                bool ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
                //SHIFT
                bool shift = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
                //F
                bool f = Window.Current.CoreWindow.GetKeyState(VirtualKey.F).HasFlag(CoreVirtualKeyStates.Down);
                if (ctrl && shift && f)
                {
                    FocusModeSwitch = !FocusModeSwitch;
                }
            };

            // Wherever you need
            Window.Current.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
            {
                e.Handled = true;
                if (FocusModeSwitch)
                {
                    FocusModeSwitch = false;
                }
            };

            //ask user if they want to save before closing the app
            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += async (sender, e) =>
            {
                var deferral = e.GetDeferral();

                if (!Changed)
                {
                    //No change made, either new document or file saved
                    deferral.Complete();
                }
                else
                {
                    if (totalCharacters < 1)
                    {
                        deferral.Complete();
                    }
                }

                //close dialogs so the app does not hang
                WantToSave.Hide();
                GoToDialog.Hide();

                await WantToSave.ShowAsync();

                switch (WantToSave.DialogResult)
                {
                    case DialogResult.Yes:
                        await SaveWork();
                        deferral.Complete();
                        break;
                    case DialogResult.No:
                        deferral.Complete();
                        break;
                    case DialogResult.Cancel:
                        e.Handled = true;
                        deferral.Complete();
                        break;
                }
            };

            CheckPushNotifications(); //check for push notifications
            AddJumplists(); //reset the jumplist tasks

            this.Loaded += MainPage_Loaded;
            this.LayoutUpdated += MainPage_LayoutUpdated;
            //
            //Match the formatted text with the initial content
            //As it technically not empty but contain format size text
            SetANewChange();
        }

        #region Startup and function handling (Main_Loaded, Uodate UI, Launch sub function, Navigation hangler
        private async void ClipboardStatusUpdate(object sender, object e)
        {
            try
            {
                DataPackageView clipboardContent = Clipboard.GetContent();
                Clipboard.ContentChanged -= ClipboardStatusUpdate;
                var dataPackage = new DataPackage();
                if (QSetting.PasteTextOnly)
                {
                    dataPackage.SetText(await clipboardContent.GetTextAsync());
                }
                else
                {
                    dataPackage = new DataPackage();
                    if (clipboardContent.Contains(StandardDataFormats.Rtf))
                        dataPackage.SetRtf(await clipboardContent.GetRtfAsync());
                    else
                        dataPackage.SetText(await clipboardContent.GetTextAsync());
                }
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
                Clipboard.ContentChanged += ClipboardStatusUpdate;
                CanPasteText = clipboardContent.Contains(StandardDataFormats.Text);
            }
            catch (Exception)
            {
                CanPasteText = false;
            }
        }

        private void UpdateAutoSaveInterval(int to)
        {
            if (timer != null)
            {
                timer.Interval = to * 1000;
            }
        }

        private void UpdateFontColor(Color to)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = to;
        }

        private void UpdateAutoSave(bool to)
        {
            if (to)
            {
                timer.Enabled = true;
                timer.Start();
            }
            else
            {
                timer.Enabled = false;
                timer.Stop();
            }
        }

        private void UpdateUIAccordingToNewTheme(object sender, ThemeChangedEventArgs e)
        {
            var to = e.VisualTheme.Theme;
            QSetting.CustomThemeId = e.ActualTheme.ThemeId;

            if (e.ActualTheme.ThemeId == "default")
                e.ActualTheme.UpdateBaseBackground(sender, e);

            //Is it dark theme or light theme? Just in case if it default, get a theme info from application
            bool isDarkTheme = to == ElementTheme.Dark;
            if (to == ElementTheme.Default)
            {
                isDarkTheme = App.Current.RequestedTheme == ApplicationTheme.Dark;
            }

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = e.VisualTheme.DefaultTextForeground;

            //Update combobox items font color collection
            if (QSetting.DefaultFontColor == "Default")
            {
                Text1.Document.Selection.CharacterFormat.ForegroundColor = e.VisualTheme.DefaultTextForeground;
                //Force a new change IF there are no change made yet
                if (!Changed)
                {
                    SetANewChange();
                }
            }
            //Update dialog theme
            WantToSave.RequestedTheme = to;

            //CommandBars and ContextMenus
            Style commandBarStyle = new Style { TargetType = typeof(CommandBarOverflowPresenter) };
            commandBarStyle.Setters.Add(new Setter(BackgroundProperty, e.VisualTheme.InAppAcrylicBrush));
            CommandBar1.CommandBarOverflowPresenterStyle = commandBarStyle;
            CommandBar2.CommandBarOverflowPresenterStyle = commandBarStyle;
            Style menuFlyoutStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };
            menuFlyoutStyle.Setters.Add(new Setter(BackgroundProperty, e.VisualTheme.InAppAcrylicBrush));
            settingsFlyoutMenu.MenuFlyoutPresenterStyle = menuFlyoutStyle;
        }

        private void UpdateText1FontSize(int to)
        {
            Text1.Document.Selection.CharacterFormat.Size = to; //set the font size
        }

        // You can also use a lambda for this
        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType != CoreAcceleratorKeyEventType.KeyDown &&
                args.EventType != CoreAcceleratorKeyEventType.SystemKeyDown &&
                Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down) &&
                args.VirtualKey != VirtualKey.Menu && args.VirtualKey == VirtualKey.Up && FocusModeSwitch == false)
            {
                CompactOverlaySwitch = true;
            }
        }

        public void LaunchCheck()
        {
            if (QSetting.AutoPickMode & CurrentWorkingFile != null)
            {
                if (CurrentWorkingFile.FileType == ".txt")
                {
                    ClassicModeSwitch = true;
                    FocusModeSwitch = false;
                    CompactOverlaySwitch = false;
                    return;
                }
                else if (CurrentWorkingFile.FileType == ".rtf")
                {
                    ClassicModeSwitch = false; ;
                    FocusModeSwitch = false;
                    CompactOverlaySwitch = false;
                    return;
                }
            }

            //check what mode to launch the app in
            switch ((AvailableModes)QSetting.LaunchMode)
            {
                case AvailableModes.Focus:
                    SwitchFocusMode(true);
                    break;
                case AvailableModes.OnTop:
                    SwitchCompactOverlayMode(true);
                    break;
                case AvailableModes.Classic:
                    SwitchClassicMode(true);
                    break;
            }
            //Check and inform font picker change
            CheckAndInformAboutCommandBar3();
        }

        private void LoadSettings()
        {
            //check if auto save is on or off
            //start auto save timer
            timer = new System.Timers.Timer(QSetting.AutoSaveInterval * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(send);
            timer.AutoReset = true;
            if (QSetting.AutoSave)
            {
                timer.Enabled = true;
                timer.Start();
            }
            else
            {
                timer.Enabled = false;
            }

            QSetting.NewUser++;
            if (QSetting.NewUser == 2)
            {
                NewUserFeedbackAsync(); //call method that asks user if they want to review the app
            }
        }

        private async void AddJumplists()
        {
            var all = await JumpList.LoadCurrentAsync();

            all.SystemGroupKind = JumpListSystemGroupKind.Recent;
            if (all.Items != null && all.Items.Count == 3)
            {
                //Clear Jumplist
                all.Items.Clear();
            }

            var focus = JumpListItem.CreateWithArguments("quickpad://focus", "ms-resource:///Resources/LaunchInFocusMode");
            focus.Description = "ms-resource:///Resources/LaunchInFocusModeDesc";
            focus.Logo = new Uri("ms-appx:///Assets/jumplist/focus.png");
            var overlay = JumpListItem.CreateWithArguments("quickpad://overlay", "ms-resource:///Resources/LaunchInOnTopMode");
            overlay.Description = "ms-resource:///Resources/LaunchInOnTopModeDesc";
            overlay.Logo = new Uri("ms-appx:///Assets/jumplist/ontop.png");
            var classic = JumpListItem.CreateWithArguments("quickpad://classic", "ms-resource:///Resources/LaunchInClassicMode");
            classic.Description = "ms-resource:///Resources/LaunchInClassicModeDesc";
            classic.Logo = new Uri("ms-appx:///Assets/jumplist/classic.png");
            all.Items.Add(focus);
            all.Items.Add(overlay);
            all.Items.Add(classic);

            try //There's quite a small chance that sometime it save and app can crash, better safe than sorry :/
            {
                await all.SaveAsync();
            }
            catch
            {
                return;
            }
        }

        // Stuff for putting the focus on the content
        private void MainPage_LayoutUpdated(object sender, object e)
        {
            if (_isPageLoaded == true)
            {
                Text1.Focus(FocusState.Programmatic); // Set focus on the main content so the user can start typing right away

                //set default font to UIs that still not depend on binding
                //Fonts.PlaceholderText = QSetting.DefaultFont;
                //Fonts.SelectedItem = QSetting.DefaultFont;
                //FontSelected.Text = Convert.ToString(Fonts.SelectedItem);
                Text1.Document.Selection.CharacterFormat.Name = QSetting.DefaultFont;

                Text1.Document.Selection.CharacterFormat.Size = QSetting.DefaultFontSize;

                LaunchCheck(); //call method to check what mode the app should launch in

                _isPageLoaded = false;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = true;
        }

        public void send(object source, System.Timers.ElapsedEventArgs e)
        {
            //Not sure if this is the cause but it might he..
            if (QuickPad.Dialog.SaveChange.IsOpen)
            {
                //There are dialog asking to save change right now
                //Abort
                return;
            }
            //timer for auto save
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (CurrentWorkingFile != null)
                {
                    try
                    {
                        var result = CurrentWorkingFile.FileType; //find out the file extension
                        if ((result.ToLower() != ".rtf"))
                        {
                            //tries to update file if it exsits and is not read only
                            Text1.Document.GetText(TextGetOptions.None, out var value);
                            await PathIO.WriteTextAsync(CurrentWorkingFile.Path, value);
                        }
                        if (result.ToLower() == ".rtf")
                        {
                            //tries to update file if it exsits and is not read only
                            Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                            await PathIO.WriteTextAsync(CurrentWorkingFile.Path, value);
                        }
                        Changed = false;
                        SetANewChange();
                    }
                    catch (Exception) { }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        #endregion

        #region Properties
        string _fn = null;
        public string CurrentFontName
        {
            get
            {
                if (_fn is null)
                    _fn = QSetting.DefaultFont;
                return _fn;
            }
            set => Set(ref _fn, value);
        }

        public void BindBackFontSelection(object selection)
        {
            if (selection is FontFamilyItem f)
            {
                Text1.Document.BeginUndoGroup();
                Text1.Document.Selection.CharacterFormat.Name = f.Name;
                CurrentFontName = f.Name;
                Text1.Document.EndUndoGroup();
            }
        }

        Color? _fc = null;
        public Color CurrentFontColor
        {
            get
            {
                if (_fc is null)
                {
                    if (QSetting.DefaultFontColor == "Default")                    
                        _fc = new UISettings().GetColorValue(UIColorType.Foreground);                    
                    else if (QSetting.DefaultFontColor.StartsWith("#"))
                        _fc = Converter.GetColorFromHex(QSetting.DefaultFontColor);
                    else
                        _fc = (Color)XamlBindingHelper.ConvertValue(typeof(Color), QSetting.DefaultFontColor);
                    Text1.Document.Selection.CharacterFormat.ForegroundColor = _fc.Value;
                }
                return _fc.Value;
            }
            set
            {
                if (!Equals(_fc, value))
                {
                    Set(ref _fc, value);
                    //
                    Text1.Document.BeginUndoGroup();
                    Text1.Document.Selection.CharacterFormat.ForegroundColor = value;
                    Text1.Document.EndUndoGroup();
                }
            }
        }

        int? _vsize = null;
        public int VisualFontSize
        {
            get
            {
                if (_vsize is null)
                {
                    _vsize = QSetting.DefaultFontSize;
                }
                return _vsize.Value;
            }
            set => Set(ref _vsize, value);
        }

        bool _paste;
        public bool CanPasteText
        {
            get => _paste;
            set => Set(ref _paste, value);
        }

        ObservableCollection<FontFamilyItem> _fonts;
        public ObservableCollection<FontFamilyItem> AllFonts
        {
            get => _fonts;
            set => Set(ref _fonts, value);
        }

        string _file_name = null;
        public string CurrentFilename
        {
            get
            {
                if (string.IsNullOrEmpty(_file_name))
                {
                    if (Changed)
                    {
                        return $"*{textResource.GetString("NewDocument")}";
                    }
                    return textResource.GetString("NewDocument");
                }
                else
                {
                    if (Changed)
                    {
                        return $"*{_file_name}";
                    }
                }
                return _file_name;
            }
            set
            {
                if (!Equals(_file_name, value))
                {
                    Set(ref _file_name, value);
                    UpdateAppTitlebar();
                }
            }
        }

        void UpdateAppTitlebar()
        {
            //Set Title bar
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = CurrentFilename;
        }

        bool _changed;
        public bool Changed
        {
            get => _changed;
            set
            {
                if (!Equals(_changed, value))
                {
                    Set(ref _changed, value);
                    NotifyPropertyChanged(nameof(CurrentFilename));
                    UpdateAppTitlebar();
                }
            }
        }

        public void CheckForChange()
        {
            //Get a formatted text to notice a change in format
            Text1.Document.GetText(TextGetOptions.FormatRtf, out string ext);
            //
            Changed = !Equals(initialLoadedContent, ext);
        }

        public async void SetANewChange()
        {
            await Task.Delay(100);
            Text1.Document.GetText(TextGetOptions.FormatRtf, out string value);
            //Set initial content
            initialLoadedContent = value;
            //Update changed
            CheckForChange();
        }

        StorageFile _file;
        public StorageFile CurrentWorkingFile
        {
            get => _file;
            set
            {
                Set(ref _file, value);
                if (value is null)
                {
                    SaveIconStatus.Visibility = Visibility.Collapsed;
                }
            }
        }
        private string key; //future access list

        private bool _isPageLoaded = false;
        private Int64 LastFontSize; //this value is the last selected characters font size

        public System.Timers.Timer timer; //this is the auto save timer interval

        bool _undo;
        public bool CanUndoText
        {
            get => _undo;
            set => Set(ref _undo, value);
        }

        bool _redo;
        public bool CanRedoText
        {
            get => _redo;
            set => Set(ref _redo, value);
        }

        #endregion

        #region Store service
        public async void CheckPushNotifications()
        {
            //regisiter for push notifications
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

        public async void NewUserFeedbackAsync()
        {
            ContentDialog deleteFileDialog = new ContentDialog //brings up a content dialog
            {
                Title = textResource.GetString("NewUserFeedbackTitle"),//"Do you enjoy using Quick Pad?",
                Content = textResource.GetString("NewUserFeedbackContent"),//"Please consider leaving a review for Quick Pad in the store.",
                PrimaryButtonText = textResource.GetString("NewUserFeedbackYes"),//"Yes",
                CloseButtonText = textResource.GetString("NewUserFeedbackNo"),//"No"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync(); //get the results if the user clicked to review or not

            if (result == ContentDialogResult.Primary)
            {
                await ShowRatingReviewDialog(); //show the review dialog.

                //log even in app center
                Analytics.TrackEvent("Pressed review from popup in app");
            }
        }

        public async Task<bool> ShowRatingReviewDialog()
        {
            try
            {
                StoreSendRequestResult result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, String.Empty);

            }
            catch (Exception)
            {
                bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));
            }

            return true;
        }

        #endregion

        #region Handling navigation
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            switch (e.Parameter)
            {
                case IActivatedEventArgs activated://File activated
                    if (activated.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                    {
                        var fileArgs = activated as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                        string strFilePath = fileArgs.Files[0].Path;
                        var file = (StorageFile)fileArgs.Files[0];
                        await LoadFasFile(file); //call method to open the file the app was launched from
                    }
                    break;
                case string parameter:
                    if (parameter == "focus")
                    {
                        FocusModeSwitch = true;
                    }
                    else if (parameter == "overlay")
                    {
                        CompactOverlaySwitch = true;
                    }
                    else if (parameter == "classic")
                    {
                        ClassicModeSwitch = true;
                    }
                    break;
                case MainPage transfer:
                    //Transfer settings from previous page
                    Text1 = transfer.Text1;
                    CurrentWorkingFile = transfer.CurrentWorkingFile;
                    CurrentFilename = transfer.CurrentFilename;
                    initialLoadedContent = transfer.initialLoadedContent;
                    break;
            }
        }

        #endregion

        #region Load/Save file
        public async Task LoadFileIntoTextBox()
        {
            if (CurrentWorkingFile.FileType.ToLower() == ".rtf")
            {
                Windows.Storage.Streams.IRandomAccessStream randAccStream = await CurrentWorkingFile.OpenAsync(FileAccessMode.Read);

                key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(CurrentWorkingFile); //let file be accessed later

                // Load the file into the Document property of the RichEditBox.
                Text1.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
            }
            else
            {
                key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(CurrentWorkingFile); //let file be accessed later

                Text1.Document.SetText(TextSetOptions.None, await FileIO.ReadTextAsync(CurrentWorkingFile));
            }
            //Clear undo/redo history
            Text1.TextDocument.ClearUndoRedoHistory();
            //Update the initial loaded content
            SetANewChange();
        }

        private async Task LoadFasFile(StorageFile inputFile)
        {
            try
            {
                CurrentWorkingFile = inputFile;
                await LoadFileIntoTextBox();
                CurrentFilename = CurrentWorkingFile.DisplayName;
            }
            catch (Exception) { }
        }

        public async Task SaveWork()
        {
            try
            {
                Text1.Document.GetText(
                    CurrentWorkingFile.FileType.ToLower() == ".rtf" ? TextGetOptions.FormatRtf : TextGetOptions.None,
                    out var value);
                await PathIO.WriteTextAsync(CurrentWorkingFile.Path, value);
                Changed = false;
            }

            catch (Exception)

            {
                Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                };

                // Dropdown of file types the user can save the file as
                //check if default file type is .txt
                if (QSetting.DefaultFileType == ".txt")
                {
                    savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                }
                else if (QSetting.DefaultFileType == ".rtf")
                {
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                    savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
                }
                savePicker.FileTypeChoices.Add("All Files", new List<string>() { "." });

                // Default file name if the user does not type one in or select a file to replace
                if (_file_name == null)
                    savePicker.SuggestedFileName = $"{textResource.GetString("NewDocument")}{QSetting.NewFileAutoNumber}";
                else
                    savePicker.SuggestedFileName = _file_name;

                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    //Change has been saved
                    Changed = false;
                    //Set the current working file
                    CurrentWorkingFile = file;
                    //update title bar
                    CurrentFilename = file.DisplayName;

                    key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

                    //save as plain text for text file
                    if ((file.FileType.ToLower() != ".rtf"))
                    {
                        Text1.Document.GetText(TextGetOptions.None, out var value); //get the text to save
                        await FileIO.WriteTextAsync(file, value); //write the text to the file
                    }
                    //save as rich text for rich text file
                    if (file.FileType.ToLower() == ".rtf")
                    {
                        Text1.Document.GetText(TextGetOptions.FormatRtf, out var value); //get the text to save
                        await FileIO.WriteTextAsync(file, value); //write the text to the file
                    }

                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //let user know if there was an error saving the file
                        Windows.UI.Popups.MessageDialog errorBox = new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                        await errorBox.ShowAsync();
                    }

                    //Increase new auto number, so next file will not get the same name
                    QSetting.NewFileAutoNumber++;
                }
                else
                {
                    CurrentWorkingFile = temporaryForce;
                    temporaryForce = null;
                }
            }
            //Update the initial loaded content
            SetANewChange();
        }
        #endregion

        #region Command bar click
        public void SetTheme(object sender, RoutedEventArgs e)
        {
            QSetting.Theme = (ElementTheme)Enum.Parse(typeof(ElementTheme), (sender as RadioButton).Tag as string);
        }

        private void SetFormatColor(object sender, RoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), tag);
        }

        private void CmdSettings_Click(object sender, RoutedEventArgs e)
        {
            MainView.IsPaneOpen = !MainView.IsPaneOpen;
            //ContentDialogResult result = await Settings.ShowAsync();
        }

        private void Justify_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = Windows.UI.Text.ParagraphAlignment.Justify;
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Right;
        }

        private void Center_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        }

        private async void CmdNew_Click(object sender, RoutedEventArgs e)
        {
            if (Changed)
            {
                //File has not been save yet ask use if they want to save
                await WantToSave.ShowAsync();

                switch (WantToSave.DialogResult)
                {
                    case DialogResult.Yes:
                        //Save change
                        await SaveWork();
                        //Clear text
                        Text1.Document.SetText(TextSetOptions.None, string.Empty);
                        break;
                    case DialogResult.No:
                        //Clear text
                        Text1.Document.SetText(TextSetOptions.None, string.Empty);
                        break;
                    case DialogResult.Cancel:
                        //ABORT
                        return;
                }
            }
            else
            {
                //File have been saved! And no change has been made. Reset right away
                Text1.Document.SetText(TextSetOptions.None, string.Empty);
            }
            if (QSetting.DefaultFileType == ".rtf")
            {
                UpdateText1FontSize(QSetting.DefaultFontSize);
            }
            //reset the value of the friendly file name
            CurrentWorkingFile = null;
            //update the title bar to reflect it is a new document
            CurrentFilename = null;
            //Clear undo and redo
            if (App.Is1903OrNewer)
            {
                if (Text1.TextDocument.CanUndo())//Assume because the history is already empty?
                {
                    Text1.TextDocument.ClearUndoRedoHistory();
                }
            }
            //Put up a default font size into a format
            UpdateText1FontSize(QSetting.DefaultFontSize);
            //Set new change
            SetANewChange();
        }

        private async void CmdOpen_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker open = new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf"); //add file type to the file picker
            open.FileTypeFilter.Add(".txt"); //add file type to the file picker
            open.FileTypeFilter.Add("*"); //add wild card so more file types can be opened

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    Windows.Storage.Streams.IRandomAccessStream randAccStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

                    if ((file.FileType.ToLower() != ".rtf"))
                    {
                        Text1.Document.SetText(Windows.UI.Text.TextSetOptions.None, await FileIO.ReadTextAsync(file));
                    }
                    if (file.FileType.ToLower() == ".rtf")
                    {
                        Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
                    }

                    //Set current file
                    CurrentWorkingFile = file;
                    CurrentFilename = CurrentWorkingFile.DisplayName;
                    //Clear undo and redo, so the last undo will be the loaded text
                    Text1.TextDocument.ClearUndoRedoHistory();
                    //Update the initial loaded content
                    SetANewChange();
                }
                catch (Exception)
                {
                    ContentDialog errorDialog = new ContentDialog()
                    {
                        Title = "File open error",
                        Content = "Sorry, Quick Pad couldn't open the file.",
                        PrimaryButtonText = "Ok"
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        public async void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            await SaveWork(); //call the function to save
        }

        private StorageFile temporaryForce = null;
        public async void CmdSaveAs_Click(object sender, RoutedEventArgs e)
        {
            temporaryForce = CurrentWorkingFile;
            CurrentWorkingFile = null;
            await SaveWork();
        }

        public async void CmdExit_Click(object sender, RoutedEventArgs e)
        {
            if (!Changed)
            {
                App.Current.Exit();
                return;
            }
            await WantToSave.ShowAsync();
            switch (WantToSave.DialogResult)
            {
                case DialogResult.Yes:
                    await SaveWork();
                    break;
                case DialogResult.Cancel:
                    return;
            }
            App.Current.Exit();
        }

        private void CmdUndo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Undo(); //undo changes the user did to the text            
            CheckForChange(); //Check fof a change in document
        }

        private void CmdRedo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Redo(); //redo changes the user did to the text          
            CheckForChange(); //Check fof a change in document
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            //set the selected text to be bold if not already
            //if the text is already bold it will make it regular
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Bold = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Text1.Document.EndUndoGroup();
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            //set the selected text to be in italics if not already
            //if the text is already in italics it will make it regular
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Italic = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Text1.Document.EndUndoGroup();
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            //set the selected text to be underlined if not already
            //if the text is already underlined it will make it regular
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                if (charFormatting.Underline == Windows.UI.Text.UnderlineType.None)
                {
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.Single;
                }
                else
                {
                    charFormatting.Underline = Windows.UI.Text.UnderlineType.None;
                }
                selectedText.CharacterFormat = charFormatting;
            }
            Text1.Document.EndUndoGroup();
        }

        private void Delete_Click(object sender, RoutedEventArgs e) => Text1.TextDocument.Selection.Text = string.Empty;

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (QSetting.PasteTextOnly)
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    //if there is nothing to paste then dont paste anything since it will crash
                    if (!string.IsNullOrEmpty(await dataPackageView.GetTextAsync()))
                    {
                        Text1.Document.Selection.TypeText(await dataPackageView.GetTextAsync()); //paste the text from the clipboard
                    }
                }
            }
            else
            {
                Text1.Document.Selection.Paste(0);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            //send the selected text to the clipboard
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text1.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            //deletes the selected text but sends it to the clipboard to be pasted somewhere else
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Text1.Document.Selection.Text);
            Text1.Document.Selection.Text = "";
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }

        private void SizeUp_Click(object sender, RoutedEventArgs e)
        {
            LastFontSize = Convert.ToInt64(Text1.Document.Selection.CharacterFormat.Size);
            //
            Text1.Document.BeginUndoGroup();
            try
            {
                //makes the selected text font size bigger
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size + 2;
            }
            catch (Exception)
            {
                Text1.Document.Selection.CharacterFormat.Size = LastFontSize;
            }
            finally
            {
                VisualFontSize = Convert.ToInt32(Text1.Document.Selection.CharacterFormat.Size);
            }
            Text1.Document.EndUndoGroup();
        }

        private void SizeDown_Click(object sender, RoutedEventArgs e)
        {
            LastFontSize = Convert.ToInt64(Text1.Document.Selection.CharacterFormat.Size);
            //
            Text1.Document.BeginUndoGroup();
            //checks if the font size is too small
            if (Text1.Document.Selection.CharacterFormat.Size > 4)
            {
                //make the selected text font size smaller
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size - 2;
            }
            Text1.Document.EndUndoGroup();
            VisualFontSize = Convert.ToInt32(Text1.Document.Selection.CharacterFormat.Size);
        }

        private void Emoji_Clicked(object sender, RoutedEventArgs e)
        {
            Text1.Focus(FocusState.Programmatic);
            try //More error here
            {
                CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji);
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent($"Attempting to open emoji keyboard\r\n{ex.Message}");
            }
        }

        StorageFile file;
        private async void CmdShare_Click(object sender, RoutedEventArgs e)
        {
            file = null;
            if (CurrentWorkingFile is null || Changed)
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile = await storageFolder.CreateFileAsync("Quick_Pad_Shared.rtf", CreationCollisionOption.ReplaceExisting);
                Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                await FileIO.WriteTextAsync(storageFile, value);
                file = storageFile;
            }
            DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            Text1.Document.GetText(TextGetOptions.UseCrlf, out var value);
            if (file is null) file = CurrentWorkingFile;

            if (!string.IsNullOrEmpty(value))
            {
                ObservableCollection<StorageFile> files = new ObservableCollection<StorageFile>();
                files.Add(file);
                args.Request.Data.Properties.Title = Package.Current.DisplayName;
                args.Request.Data.SetStorageItems(files);
            }
            else
            {
                //"Nothing to share, type something in order to share it."
                args.Request.FailWithDisplayText(textResource.GetString("NothingToShare"));
            }
        }

        private async void CmdReview_Click(object sender, RoutedEventArgs e)
        {
            await ShowRatingReviewDialog(); //show the review dialog.

            Analytics.TrackEvent("User clicked on review"); //log even in app center
        }

        private void Strikethrough_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Strikethrough = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Text1.Document.EndUndoGroup();
        }

        private void BulletList_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            if (Text1.Document.Selection.ParagraphFormat.ListType == MarkerType.Bullet)
            {
                Text1.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
            }
            else
            {
                Text1.Document.Selection.ParagraphFormat.ListType = MarkerType.Bullet;
            }
            Text1.Document.EndUndoGroup();
        }

        private void ShowFontsDialog_Click(object sender, RoutedEventArgs e)
        {
            //Show setting pane w/ a font page
            Dialog.Settings.forceStartupToPage = Dialog.settingPage.Font;
            MainView.IsPaneOpen = true;
        }

        private void ClosingCustomizeFlyout(Windows.UI.Xaml.Controls.Primitives.FlyoutBase sender, Windows.UI.Xaml.Controls.Primitives.FlyoutBaseClosingEventArgs args)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
                args.Cancel = true;
        }

        public void ResetToolbarSetting()
        {
            QSetting.ShowFont =
            QSetting.ShowColor =
            QSetting.ShowEmoji =
            QSetting.ShowBold =
            QSetting.ShowItalic =
            QSetting.ShowUnderline =
            QSetting.ShowStrikethroughOption =
            QSetting.ShowBullets =
            QSetting.ShowAlignLeft =
            QSetting.ShowAlignCenter =
            QSetting.ShowAlignRight =
            QSetting.ShowAlignJustify =
            QSetting.ShowSizeUp =
            QSetting.ShowSizeDown =
            true;
        }

        private void OpenFontFlyout(object sender, object e)
        {
            trySelectFontName = "";
            FontListSelection.Focus(FocusState.Programmatic);
            FontListSelection.ScrollIntoView(FontListSelection.SelectedItem);
        }

        string trySelectFontName;
        private void TryToFindFont(UIElement sender, CharacterReceivedRoutedEventArgs args)
        {
            trySelectFontName += args.Character;
            var trySelect = AllFonts.FirstOrDefault(i => i.Name.ToLower().StartsWith(trySelectFontName.ToLower()));
            if (trySelect is null)
                return;

            FontListSelection.ScrollIntoView(trySelect, ScrollIntoViewAlignment.Leading);
            FontListSelection.SelectedItem = trySelect;
        }

        private void ColorSelection_ColorSelectionChanged(object sender, Dialog.ColorSelectionChangedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = e.SelectedColor;
        }

        #endregion

        #region UI Mode change
        bool? _compact = null;
        public bool CompactOverlaySwitch
        {
            get
            {
                if (_compact is null)
                {
                    _compact = QSetting.LaunchMode == (int)AvailableModes.OnTop;
                }
                return _compact.Value;
            }
            set
            {
                Set(ref _compact, value);
                SwitchCompactOverlayMode(value);
                CheckAndInformAboutCommandBar3();
            }
        }

        bool? _focus = null;
        public bool FocusModeSwitch
        {
            get
            {
                if (_focus is null)
                {
                    _focus = QSetting.LaunchMode == (int)AvailableModes.Focus;
                }
                return _focus.Value;
            }
            set
            {
                Set(ref _focus, value);
                SwitchFocusMode(value);
                CheckAndInformAboutCommandBar3();
            }
        }

        bool? _classic = null;
        public bool ClassicModeSwitch
        {
            get
            {
                if (_classic is null)
                {
                    _classic = QSetting.LaunchMode == (int)AvailableModes.Classic;
                }
                return _classic.Value;
            }
            set
            {
                Set(ref _classic, value);
                SwitchClassicMode(value);
                CheckAndInformAboutCommandBar3();
            }
        }

        public async void SwitchCompactOverlayMode(bool switching)
        {
            if (switching)
            {
                ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                compactOptions.CustomSize = new Windows.Foundation.Size(QSetting.CompactSizeWidth, QSetting.CompactSizeHeight);
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

                CommandBar1.Visibility = Visibility.Collapsed;
                FrameTop.Visibility = Visibility.Collapsed;
                CmdSettings.Visibility = Visibility.Collapsed;
                CmdFocusMode.Visibility = Visibility.Collapsed;
                CmdClassicMode.Visibility = Visibility.Collapsed;
                CommandBarClassic.Visibility = Visibility.Collapsed;
                Shadow1.Visibility = Visibility.Collapsed;
                CommandBar2.Visibility = Visibility.Collapsed;
                StatusBar.Visibility = Visibility.Collapsed;
                FileTitle.Visibility = Visibility.Collapsed;
                trickyTitleBar.Margin = new Thickness(33, 0, 0, 0);
                StatusBarShadow.Visibility = Visibility.Collapsed;

                //Hide Find and Replace dialog if it open
                ShowFindAndReplace = false;

                //log even in app center
                Analytics.TrackEvent("Compact Overlay");
            }
            else
            {
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

                CommandBar1.Visibility = Visibility.Visible;
                Shadow1.Visibility = Visibility.Visible;
                FrameTop.Visibility = Visibility.Visible;
                CmdSettings.Visibility = Visibility.Visible;
                CmdFocusMode.Visibility = Visibility.Visible;
                CmdClassicMode.Visibility = Visibility.Visible;
                CommandBar2.Visibility = Visibility.Visible;
                StatusBar.Visibility = Visibility.Visible;
                FileTitle.Visibility = Visibility.Visible;

                QSetting.CompactSizeHeight = Convert.ToInt16(ActualHeight);
                QSetting.CompactSizeWidth = Convert.ToInt16(ActualWidth);

                if (ClassicModeSwitch == true)
                {
                    SwitchClassicMode(true);
                    CommandBarClassic.Visibility = Visibility.Visible;
                    if (QSetting.ShowStatusBar == true)
                    {
                        StatusBarShadow.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public void SwitchFocusMode(bool switching)
        {
            if (switching)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                Text1.SetValue(Canvas.ZIndexProperty, 90);
                CommandBar1.Visibility = Visibility.Collapsed;
                CommandBar2.Visibility = Visibility.Collapsed;
                Shadow1.Visibility = Visibility.Collapsed;
                row1.Height = new GridLength(0);
                row3.Height = new GridLength(0);
                QSetting.TimesUsingFocusMode++;
                HowToLeaveFocus.IsOpen = QSetting.TimesUsingFocusMode < 2;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;
                Text1.SetValue(Canvas.ZIndexProperty, 0);
                CommandBar2.Visibility = Visibility.Visible;
                CommandBar1.Visibility = Visibility.Visible;
                Shadow1.Visibility = Visibility.Visible;
                CommandBarClassic.Visibility = Visibility.Collapsed;
                row1.Height = new GridLength(1, GridUnitType.Auto);
                row3.Height = new GridLength(1, GridUnitType.Auto);
                if (ShowFindAndReplace == true)
                {
                    ShowFindAndReplace = false;
                }
                if (HowToLeaveFocus.IsOpen)
                    HowToLeaveFocus.IsOpen = false;
            }
            if (ClassicModeSwitch == true)
            {
                SwitchClassicMode(true);
                CommandBarClassic.Visibility = Visibility.Visible;
            }
        }

        public void SwitchClassicMode(bool switching)
        {
            if (switching)
            {
                CommandBar1.Visibility = Visibility.Collapsed;
                CommandBar2.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandBar1.Visibility = Visibility.Visible;
                CommandBar2.Visibility = Visibility.Visible;
            }
        }

        //Use for Click function
        public void SwitchingFocusMode() => FocusModeSwitch = !FocusModeSwitch;
        public void SwitchingOverlayMode() => CompactOverlaySwitch = !CompactOverlaySwitch;
        public void SwitchingClassicAndDefault() => ClassicModeSwitch = !ClassicModeSwitch;

        public void SwitchingStatusBarDisplay() => QSetting.ShowStatusBar = !QSetting.ShowStatusBar;

        public void CheckAndInformAboutCommandBar3()
        {
            if (!FocusModeSwitch && !CompactOverlaySwitch && !ClassicModeSwitch)//Check if app isn't in any of this mode
            {
                if (!QSetting.AcknowledgeFontSelectionChange)
                {
                    FontOptionTip.IsOpen = true;
                }
            }
        }

        public void AcknowledgeTheChangeOfFontSelection() => QSetting.AcknowledgeFontSelectionChange = true;
        #endregion

        #region Textbox function
        private void Text1_GotFocus(object sender, RoutedEventArgs e)
        {
            LastFontSize = Convert.ToInt64(Text1.Document.Selection.CharacterFormat.Size); //get font size of last selected character
        }

        private void Text1_TextChanged(object sender, RoutedEventArgs e)
        {
            CanUndoText = Text1.Document.CanUndo();
            CanRedoText = Text1.Document.CanRedo();

            CheckForChange(); //Check fof a change in document

            if (ClassicModeSwitch)
            {

            }
        }

        private void Text1_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (args.IsContentChanging)
            {
                Text1.Document.GetText(TextGetOptions.None, out _content);
                _content = TrimRichEditBoxText(_content);
                _isLineCachePendingUpdate = true;
                MaximumPossibleSearchRange = totalCharacters = _content.Length;
            }
        }
        /// <summary>
        /// Temporary store the copy of text when it loaded, 
        /// if it didn't match the textbox=it changed
        /// </summary>
        private string initialLoadedContent;

        private void Text1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (IsCttrlPressed() && e.Key == (VirtualKey)187) //ctrl + +
            {
                ScrollViewer ContentScroll = ScroolZoom;
                if (scaleValue <= 4)
                {
                    scaleValue = scaleValue + (scalePercentage / 100);
                }
                ContentScroll.ChangeView(0, 0, scaleValue);
            }
            else if (IsCttrlPressed() && e.Key == (VirtualKey)189) //ctrl + -
            {
                ScrollViewer ContentScroll = ScroolZoom;
                if (scaleValue >= 0.5)
                {
                    scaleValue = scaleValue - (scalePercentage / 100);
                }
                ContentScroll.ChangeView(0, 0, scaleValue);
            }
            else if (IsCttrlPressed() && e.Key == (VirtualKey)48) //ctrl + 0
            {
                ScrollViewer ContentScroll = ScroolZoom;
                scaleValue = 1;
                ContentScroll.ChangeView(0, 0, scaleValue);
            }

            if (e.Key == VirtualKey.Tab)
            {
                RichEditBox richEditBox = sender as RichEditBox;
                if (richEditBox != null)
                {
                    richEditBox.Document.Selection.TypeText("\t");
                    e.Handled = true;
                }
            }
            else if (e.Key == VirtualKey.Space)
            {
                Text1.Document.EndUndoGroup();
                Text1.Document.BeginUndoGroup();
            }
            if (ClassicModeSwitch)
            {
                CheckForStatusUpdate();
            }
        }

        private bool IsCttrlPressed()
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        private void Text1_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Text1_Drop(object sender, DragEventArgs e)
        {
            //Check if file is open and ask user if they want to save it when dragging a file in to Quick Pad.
            if (Changed)
            {
                //Only show save dialog when there are changed made
                await WantToSave.ShowAsync();
                switch (WantToSave.DialogResult)
                {
                    case DialogResult.Yes:
                        await SaveWork();
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }
            //At this point user totally want to open that dropped file
            //Void the previous file
            CurrentWorkingFile = null;
            //
            //load rich text files dropped in from file explorer
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        CurrentWorkingFile = items[0] as StorageFile;
                        await LoadFileIntoTextBox();
                        CurrentFilename = CurrentWorkingFile.DisplayName;

                        //log event in app center
                        Analytics.TrackEvent("Droped file in to Quick Pad");
                    }
                }
            }
            catch (Exception) { }
        }

        private void Text1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            CurrentFontName = Text1.Document.Selection.CharacterFormat.Name; //updates font box to show the selected characters font

            //Update Status bar
            CheckForStatusUpdate();
        }

        private void Text1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CheckForStatusUpdate();
        }

        #endregion

        #region Status bar and update
        public void CheckForStatusUpdate()
        {
            //Update line and character count
            GetCurrentLineColumn(out int lineIndex, out int columnIndex, out int selectedCount);
            CurrentPosition = columnIndex;
            CurrentLine = lineIndex;
            SelectionLength = selectedCount;

            //update current format
            IsItBold = Text1.Document.Selection.CharacterFormat.Bold == FormatEffect.On;
            IsItItalic = Text1.Document.Selection.CharacterFormat.Italic == FormatEffect.On;
            IsItUnderline = Text1.Document.Selection.CharacterFormat.Underline != UnderlineType.None;
            IsItStrikethrough = Text1.Document.Selection.CharacterFormat.Strikethrough == FormatEffect.On;
            IsUsingBulletList = Text1.Document.Selection.ParagraphFormat.ListType != MarkerType.None;
            //Selection update
            if (Text1.Document.Selection is null)
            {
                SelectionLength = 0;
            }
            else
            {
                if (Text1.Document.Selection.Length < 0)
                {
                    SelectionLength = Text1.Document.Selection.Length * -1;
                }
                else
                {
                    SelectionLength = Text1.Document.Selection.Length;
                }
            }
        }

        public void GetCurrentLineColumn(out int lineIndex, out int columnIndex, out int selectedCount)
        {
            if (_isLineCachePendingUpdate)
            {
                _contentLinesCache = (_content + RichEditBoxDefaultLineEnding).Split(RichEditBoxDefaultLineEnding);
                _isLineCachePendingUpdate = false;
            }

            var start = Text1.Document.Selection.StartPosition;
            var end = Text1.Document.Selection.EndPosition;

            lineIndex = 1;
            columnIndex = 1;
            selectedCount = 0;

            var length = 0;
            bool startLocated = false;
            for (int i = 0; i < _contentLinesCache.Length; i++)
            {
                var line = _contentLinesCache[i];

                if (line.Length + length >= start && !startLocated)
                {
                    lineIndex = i + 1;
                    columnIndex = start - length + 1;
                    startLocated = true;
                }

                if (line.Length + length >= end)
                {
                    if (i == lineIndex - 1)
                        selectedCount = end - start;
                    else
                        selectedCount = end - start + (i - lineIndex);
                    return;
                }

                length += line.Length + 1;
            }
        }

        private string TrimRichEditBoxText(string text)
        {
            // Trim end \r
            if (!string.IsNullOrEmpty(text) && text[text.Length - 1] == RichEditBoxDefaultLineEnding)
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        private int _line;
        /// <summary>
        /// Line count
        /// </summary>
        public int totalLine
        {
            get => _line;
            set => Set(ref _line, value);
        }

        private int _char;
        /// <summary>
        /// Character count
        /// </summary>
        public int totalCharacters
        {
            get => _char;
            set => Set(ref _char, value);
        }

        private int _cp;
        public int CurrentPosition
        {
            get => _cp;
            set => Set(ref _cp, value);
        }

        private int _cl;
        public int CurrentLine
        {
            get => _cl;
            set => Set(ref _cl, value);
        }

        private int _selTT;
        public int SelectionLength
        {
            get => _selTT;
            set => Set(ref _selTT, value);
        }

        private bool _bold, _italic, _underline;
        public bool IsItBold
        {
            get => _bold;
            set => Set(ref _bold, value);
        }

        public bool IsItItalic
        {
            get => _italic;
            set => Set(ref _italic, value);
        }

        public bool IsItUnderline
        {
            get => _underline;
            set => Set(ref _underline, value);
        }

        private bool _st;
        public bool IsItStrikethrough
        {
            get => _st;
            set => Set(ref _st, value);
        }

        private bool _bs;
        public bool IsUsingBulletList
        {
            get => _bs;
            set => Set(ref _bs, value);
        }
        #endregion

        #region Find & Replace
        bool _fr;
        public bool ShowFindAndReplace
        {
            get => _fr;
            set
            {
                if (!Equals(_fr, value))
                {
                    if (CompactOverlaySwitch && value)
                    {
                        //Atempt to open find and replace on compact overlay
                        return;//Abort
                    }
                    Set(ref _fr, value);
                    if (value)
                    {
                        if (Text1.Document.Selection.Length > 1)
                        {
                            FindAndReplaceDialog.TextToFind = Text1.Document.Selection.Text;
                            DelayFocus();
                        }
                        else
                        {
                            FindAndReplaceDialog.TextToFind = "";
                            DelayFocus();
                        }
                        FindAndReplaceDialog.onRequestFinding += FindRequestedText;
                        FindAndReplaceDialog.onRequestReplacing += FindAndReplaceRequestedText;
                        FindAndReplaceDialog.onClosed += ToggleFindAndReplaceDialog;
                    }
                    else
                    {
                        FindAndReplaceDialog.onRequestFinding -= FindRequestedText;
                        FindAndReplaceDialog.onRequestReplacing -= FindAndReplaceRequestedText;
                        FindAndReplaceDialog.onClosed -= ToggleFindAndReplaceDialog;

                        //Collapsed the replace after close dialog
                        FindAndReplaceDialog.ShowReplace = false;
                    }
                }
            }
        }

        async void DelayFocus()
        {
            await Task.Delay(100);
            FindAndReplaceDialog.FindInput.Focus(FocusState.Pointer);
        }

        public void ToggleFindAndReplaceDialog()
        {
            ShowFindAndReplace = !ShowFindAndReplace;
        }

        int _ssp;
        public int StartSearchPosition
        {
            get => _ssp;
            set => Set(ref _ssp, value);
        }

        int _maxRange;
        public int MaximumPossibleSearchRange
        {
            get => _maxRange;
            set => Set(ref _maxRange, value);
        }

        private async void FindWithBing()
        {
            if (Text1.TextDocument.Selection.Text.Length > 0)
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri($"https://www.bing.com/search?q={Text1.TextDocument.Selection.Text}"));
            }
        }

        private void FindRequestedText(string find, bool direction, bool match)
        {
            if (string.IsNullOrEmpty(find))
                return;
            int start = Text1.Document.Selection.StartPosition;
            int end = Text1.Document.Selection.EndPosition;
            FindOptions matchCase = match ? FindOptions.Case : FindOptions.None;
            
            if (direction)
            {
                //Search forward
                Text1.Document.Selection.FindText(find, MaximumPossibleSearchRange, matchCase);
                //Check if position change, if change not, then it found nothing else it found and end it
                if (Text1.Document.Selection.StartPosition == start || Text1.Document.Selection.EndPosition == end)
                {
                    //Not found, have one more chance to wrap around and start over
                    Text1.Document.Selection.StartPosition = Text1.Document.Selection.EndPosition = 0;
                    //Search again..
                    Text1.Document.Selection.FindText(find, MaximumPossibleSearchRange, matchCase);
                }
            }
            else
            {
                //Search backward
                if (start < 1 && end < 1)
                {
                    //Start position is at the start. Go to the end of text
                    Text1.Document.Selection.StartPosition
                        = Text1.Document.Selection.EndPosition
                        = MaximumPossibleSearchRange;
                }
                //A selection length
                int result = 0;
                int length = MaximumPossibleSearchRange;
                bool retry = false; //Use as a mark to check does the search has been start from the end before? | false = no | true = yes
                while (result < 1 || result == find.Length) //Loop until the length is more than 1 or same as the length of text that want to find
                {
                    //Find a text but only at a length of text that want to find
                    result = Text1.Document.Selection.FindText(find, find.Length, matchCase);
                    if (result < 1)//Still not found a text?
                    {
                        Text1.Document.Selection.StartPosition -= find.Length; //Shift the cursor back a length of searching text
                        Text1.Document.Selection.EndPosition = Text1.Document.Selection.StartPosition;
                        if (Text1.Document.Selection.StartPosition < 1)
                        {
                            if (retry) //Does the search has been start over before?
                                break; //If it does, stop

                            //It already back to the beginning
                            //One more chance to go start from the end
                            Text1.Document.Selection.StartPosition =
                                Text1.Document.Selection.EndPosition =
                                MaximumPossibleSearchRange;
                            retry = true; //mark the search as this has been move to start over from the end
                            break; //Search over.
                        }
                    }
                    else
                    {
                        //Found a text
                        break; //Search over
                    }
                }
            }

            //Scroll to the found text
            Text1.TextDocument.Selection.ScrollIntoView(PointOptions.Start);
        }

        private void FindAndReplaceRequestedText(string find, string replace, bool direction, bool match, bool all)
        {
            if (string.IsNullOrEmpty(find))
            {
                //Nothing to search for
                return;
            }
            if (replace is null)
            {
                replace = string.Empty;
            }
            //track start and end position of cursor
            int start = Text1.Document.Selection.StartPosition;
            int end = Text1.Document.Selection.EndPosition;
            FindOptions matchCase = match ? FindOptions.Case : FindOptions.None;

            if (all)
            {
                //Replace all
                //Start from the back and work backward from there
                Text1.Document.Selection.StartPosition =
                    Text1.Document.Selection.EndPosition =
                    MaximumPossibleSearchRange;
                //Track result
                int result = 0;
                //Begin the loop of search and replace
                while (Text1.Document.Selection.StartPosition > 1) //Search and replace until the cursor hit 0,0
                {
                    result = Text1.Document.Selection.FindText(find, find.Length, matchCase); //Find a text
                    if (result >= 1)
                    {
                        //Found a text
                        //Replace
                        Text1.Document.Selection.Text = replace;
                        //Set cursor back further
                        Text1.Document.Selection.StartPosition -= (find.Length + replace.Length);
                    }
                    else
                    {
                        //Not found anything
                        //Continue
                        int testLength = Text1.Document.Selection.StartPosition - find.Length;
                        if (testLength < 1)
                        {
                            //Length less than 0. no more search can occur
                            break;
                        }
                        else
                        {
                            //Continue the search
                            Text1.Document.Selection.StartPosition = testLength;
                        }
                    }
                }
            }
            else
            {
                //Find and replace once
                //Search
                bool attemp = false; //Like a try tracking on Find function
                while (true)
                {
                    FindRequestedText(find, direction, match); //Start searching
                    //Check if it found or not
                    if (start != Text1.Document.Selection.StartPosition && end != Text1.Document.Selection.EndPosition)
                    {
                        if ((Text1.Document.Selection.StartPosition == 0 && Text1.Document.Selection.EndPosition == 0)
                            || (Text1.Document.Selection.StartPosition == MaximumPossibleSearchRange && Text1.Document.Selection.EndPosition == MaximumPossibleSearchRange))
                        {
                            if (attemp) //It has been attemp to start over before
                                break; //Stop
                                       //It's been forced to select at the start or at the end as it has found any result
                                       //Wait for the loop to try again
                            attemp = true; //Mark as it has been attemp to start over
                            //Begin new search
                            continue;
                        }
                        //Found text, replace
                        Text1.Document.Selection.Text = replace;
                        break; //Leave the loop
                    }
                }
            }
            //Set focus into text1
            Text1.Focus(FocusState.Pointer);
        }

        private void CMDSelectAll_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.GetText(TextGetOptions.None, out string value);
            Text1.Document.Selection.SetRange(0, value.Length);
        }

        private void CMDInsertDateTime_Click(object sender, RoutedEventArgs e)
        {
            var current = CultureInfo.CurrentUICulture.DateTimeFormat;
            Text1.Document.Selection.Text = $"{Text1.Document.Selection.Text}{DateTime.Now.ToString(current.ShortTimePattern)} {DateTime.Now.ToString(current.ShortDatePattern)}";
            Text1.Document.Selection.StartPosition = Text1.Document.Selection.EndPosition;
        }

        private float scaleValue = 1;
        private float scalePercentage = 10;

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            ScrollViewer ContentScroll = ScroolZoom;
            if (scaleValue <= 4)
            {
                scaleValue = scaleValue + (scalePercentage / 100);
            }
            ContentScroll.ChangeView(0, 0, scaleValue);
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            ScrollViewer ContentScroll = ScroolZoom;
            if (scaleValue >= 0.5)
            {
                scaleValue = scaleValue - (scalePercentage / 100);
            }
            ContentScroll.ChangeView(0, 0, scaleValue);
        }

        private void ResetZoom(object sender, RoutedEventArgs e)
        {
            ScrollViewer ContentScroll = ScroolZoom;
            scaleValue = 1;
            ContentScroll.ChangeView(0, 0, scaleValue);
        }

        //Menubar function
        public void OpenFindDialogWithReplace()
        {
            FindAndReplaceDialog.ShowReplace = true;
            ShowFindAndReplace = true;
        }

        private void SwitchCompactOverlayMode()
        {
            if (CompactOverlaySwitch)
                SwitchCompactOverlayMode(false);
            else
                SwitchCompactOverlayMode(true);
        }

        private void CompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            SwitchCompactOverlayMode(true);
        }

        private void CloseCompactOverlay_Click(object sender, RoutedEventArgs e)
        {
            SwitchCompactOverlayMode(false);
        }

        public async void CMDGoTo_Click()
        {
            await GoToDialog.ShowAsync();
            if (GoToDialog.finalResult == DialogResult.Yes)
            {
                Text1.TextDocument.GetText(TextGetOptions.None, out string value);
                totalLine = value.Count(i => i == '\n' || i == '\r');
                totalCharacters = value.Length;
                System.Diagnostics.Debug.WriteLine($"\"{value}\"");
                int input = int.Parse(GoToDialog.LineInput.Text) - 1;
                if (input < 0) { return; }
                if (input > totalLine - 1)
                {
                    return;
                }
                else
                {
                    int index = 0;
                    while (input > 0)
                    {
                        if (value[index] == '\r')
                        {
                            index++;
                            input--;
                            if (index == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            index++;
                        }
                    }
                    Text1.TextDocument.Selection.StartPosition = index;
                    Text1.TextDocument.Selection.EndPosition = index;
                    Text1.TextDocument.Selection.ScrollIntoView(PointOptions.Start);
                    Text1.Focus(FocusState.Pointer);
                }
            }
        }
        #endregion

        public static T MyFindRichEditBoxChildOfType<T>(DependencyObject root) where T : class
        {
            var MyQueue = new Queue<DependencyObject>();
            MyQueue.Enqueue(root);
            while (MyQueue.Count > 0)
            {
                DependencyObject current = MyQueue.Dequeue();
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    MyQueue.Enqueue(child);
                }
            }
            return null;
        }

    }

    public class FontFamilyItem : INotifyPropertyChanged, IComparable<FontFamilyItem>
    {
        private const int previewMaxLenght = 16;
        private static string _previewText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get;
            private set;
        }
        public string PreviewText
        {
            get => _previewText ?? Name;
        }

        public FontFamilyItem(string name)
        {
            Name = name;
        }

        public static void ChangeGlobalPreview(string previewText)
        {
            if (previewText != null)
            {
                previewText = previewText.Trim();
            }
            if (string.IsNullOrWhiteSpace(previewText))
            {
                _previewText = null;
            }
            else
            {
                previewText = previewText.Trim();
                if (previewText.Length > previewMaxLenght)
                {
                    _previewText = $"{previewText.Substring(0, previewMaxLenght)}...";
                }
                else
                {
                    _previewText = previewText;
                }
            }
        }
        public void UpdateLocalPreview()
        {
            UpdateProperty(nameof(PreviewText));
        }

        public int CompareTo(FontFamilyItem other)
        {
            return this.Name.CompareTo(other.Name);
        }
        public override string ToString()
        {
            return Name;
        }
        private void UpdateProperty([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DefaultLanguage
    {
        public string Name;
        public string ID;

        public DefaultLanguage()
        {
            Name = "";
            ID = "";
        }

        public DefaultLanguage(string id)
        {
            CultureInfo info = new CultureInfo(id);
            ID = info.Name;
            Name = info.DisplayName;
        }
    }

    public enum DialogResult
    {
        None,
        Yes,
        No,
        Cancel
    }
}
