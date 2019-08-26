using Microsoft.AppCenter.Analytics;
using Microsoft.Services.Store.Engagement;
using Newtonsoft.Json.Linq;
using QuickPad;
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
using Windows.Globalization;
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

        public QuickPad.Setting QSetting { get; } = new QuickPad.Setting(); //Store all app setting here..

        public QuickPad.Dialog.SaveChange WantToSave = new QuickPad.Dialog.SaveChange();
        public MainPage()
        {
            InitializeComponent();

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            //Subscribe to events
            QSetting.afterThemeChanged += UpdateUIAccordingToNewTheme;
            UpdateUIAccordingToNewTheme(QSetting.Theme);
            QSetting.afterFontSizeChanged += UpdateText1FontSize;
            UpdateText1FontSize(QSetting.DefaultFontSize);
            QSetting.afterAutoSaveChanged += UpdateAutoSave;
            //Match the formatted text with the initial content
            //As it technically not empty but contain format size text
            SetANewChange();
            //
            CreateItems();
            LoadSettings();
            LoadFonts();

            VersionNumber.Text = string.Format(textResource.GetString("VersionFormat"), Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

            //check if focus is on app or off the app
            Window.Current.CoreWindow.Activated += (sender, args) =>
            {
                if (args.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                {
                    if (CommandBar2.Visibility == Visibility.Visible)
                    {
                        CommandBar2.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                    if (CloseFocusMode.Visibility == Visibility.Visible)
                    {
                        CloseFocusMode.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
                    if (CommandBarClassic.Visibility == Visibility.Visible)
                    {
                        CommandBarClassic.Focus(FocusState.Programmatic); // Set focus off the main content
                    }
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
                    try //In case if all the change is just nothing but format
                    {
                        Text1.TextDocument.GetText(TextGetOptions.None, out string change);
                        if (string.IsNullOrEmpty(change))
                        {
                            deferral.Complete();
                        }
                    }
                    catch (Exception er)
                    {
                        //According to error report, the error is in line 132, or when Text1 try to get text
                        Analytics.TrackEvent($"Track down error \r\n{er.Message}");
                    }
                }

                //close dialogs so the app does not hang
                WantToSave.Hide();
                Settings.Hide();

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

        #region Startup and function handling (Main_Loaded, Uodate UI, Launch sub function, Navigation hangler
        private void UpdateUIAccordingToNewTheme(ElementTheme to)
        {
            //Is it dark theme or light theme? Just in case if it default, get a theme info from application
            bool isDarkTheme = to == ElementTheme.Dark;
            if (to == ElementTheme.Default)
            {
                isDarkTheme = App.Current.RequestedTheme == ApplicationTheme.Dark;
            }
            //Tell analytics what theme is selected
            if (to == ElementTheme.Default)
            {
                Analytics.TrackEvent($"Loaded app in {QSetting.Theme.ToString().ToLower()} theme from a system, which is {(isDarkTheme ? "dark theme" : "light theme")}");
            }
            else
            {
                Analytics.TrackEvent($"Loaded app in {QSetting.Theme.ToString().ToLower()} theme");
            }
            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (isDarkTheme)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }

            //Update combobox items font color collection

            if (QSetting.DefaultFontColor == "Default")
            {
                Text1.Document.Selection.CharacterFormat.ForegroundColor = isDarkTheme ? Colors.White : Colors.Black;
                //Force a new change IF there are no change made yet
                if (!Changed)
                {
                    SetANewChange();
                }
            }
            //Update dialog theme
            WantToSave.RequestedTheme = to;
        }

        private void UpdateText1FontSize(int to)
        {
            Text1.Document.Selection.CharacterFormat.Size = to; //set the font size
        }

        public void LaunchCheck()
        {
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
        }

        private ObservableCollection<DefaultLanguage> _DefaultLanguage;
        public ObservableCollection<DefaultLanguage> DefaultLanguages
        {
            get => _DefaultLanguage;
            set => Set(ref _DefaultLanguage, value);
        }

        private void CreateItems()
        {
            FontColorCollections = new ObservableCollection<FontColorItem>
            {
                new FontColorItem(),
                new FontColorItem("Black"),
                new FontColorItem("White"),
                new FontColorItem("Blue", "SkyBlue"),
                new FontColorItem("Green", "LightGreen"),
                new FontColorItem("Pink", "LightPink"),
                new FontColorItem("Yellow", "LightYellow"),
                new FontColorItem("Orange", "LightSalmon")
            };

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguage>();
            foreach (var lang in supportedLang)
            {
                DefaultLanguages.Add(new DefaultLanguage(lang));
            }
        }

        private void LoadSettings()
        {
            //check if auto save is on or off
            //start auto save timer
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

        private void LoadFonts()
        {
            //Load all fonts
            List<string> fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies().ToList();
            //Sort it in alphabet order
            fonts.Sort((fontA, fontB) => fontA.CompareTo(fontB));
            //Put it on an observable list
            AllFonts = new ObservableCollection<FontFamilyItem>(fonts.Select(x => new FontFamilyItem(x)));
        }

        private async void AddJumplists()
        {
            var all = await JumpList.LoadCurrentAsync();

            all.SystemGroupKind = JumpListSystemGroupKind.None;
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
                Fonts.PlaceholderText = QSetting.DefaultFont;
                Fonts.SelectedItem = QSetting.DefaultFont;
                FontSelected.Text = Convert.ToString(Fonts.SelectedItem);
                Text1.Document.Selection.CharacterFormat.Name = QSetting.DefaultFont;

                //check what default font color is

                if (QSetting.DefaultFontColor == "Default")
                {
                    SelectedDefaultFontColor = 0;
                }
                else
                {
                    SelectedDefaultFontColor = FontColorCollections.IndexOf(FontColorCollections.First(i => i.TechnicalName == QSetting.DefaultFontColor));
                }

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
        ObservableCollection<FontFamilyItem> _fonts;
        public ObservableCollection<FontFamilyItem> AllFonts
        {
            get => _fonts;
            set => Set(ref _fonts, value);
        }

        private void DefaultLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QSetting.AppLanguage = DefaultLanguages[(sender as ComboBox).SelectedIndex].ID;
            ApplicationLanguages.PrimaryLanguageOverride = QSetting.AppLanguage;
        }

        //Colors
        ObservableCollection<FontColorItem> _fci;
        public ObservableCollection<FontColorItem> FontColorCollections
        {
            get => _fci;
            set => Set(ref _fci, value);
        }

        public int _fc_selection = -1;
        public int SelectedDefaultFontColor
        {
            get => _fc_selection;
            set
            {
                if (!Equals(_fc_selection, value))
                {
                    Set(ref _fc_selection, value);
                    //Update setting
                    QSetting.DefaultFontColor = FontColorCollections[value].TechnicalName;
                    if (QSetting.DefaultFontColor == "Default")
                    {
                        bool isDarkTheme = RequestedTheme == ElementTheme.Dark;
                        if (RequestedTheme == ElementTheme.Default)
                        {
                            isDarkTheme = App.Current.RequestedTheme == ApplicationTheme.Dark;
                        }
                        Text1.Document.Selection.CharacterFormat.ForegroundColor = isDarkTheme ? Colors.White : Colors.Black;
                    }
                    else
                    {
                        Text1.Document.Selection.CharacterFormat.ForegroundColor = FontColorCollections[value].ActualColor;
                    }
                }
            }
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

        public void SetANewChange()
        {
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

        public System.Timers.Timer timer = new System.Timers.Timer(10000); //this is the auto save timer interval

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
            StoreSendRequestResult result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, String.Empty);

            if (result.ExtendedError == null)
            {
                JObject jsonObject = JObject.Parse(result.Response);
                if (jsonObject.SelectToken("status").ToString() == "success")
                {
                    // The customer rated or reviewed the app.
                    return true;
                }
            }

            // There was an error with the request, or the customer chose not to rate or review the app.
            return false;
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
                    savePicker.SuggestedFileName = $"{_file_name}{QSetting.NewFileAutoNumber}";
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

        private async void CmdSettings_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await Settings.ShowAsync();
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
            try
            {
                if (Text1.TextDocument.CanUndo())//Assume because the history is already empty?
                {
                    Text1.TextDocument.ClearUndoRedoHistory();
                }
            }
            catch (Exception ex)
            {
                Analytics.TrackEvent($"Error trying to clear undo history\r\n{ex.Message}");
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
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                //if there is nothing to paste then dont paste anything since it will crash
                if (text != "")
                {
                    Text1.Document.Selection.TypeText(text); //paste the text from the clipboard
                }
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            //send the selected text to the clipboard
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text1.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            //deletes the selected text but sends it to the clipboard to be pasted somewhere else
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Text1.Document.Selection.Text);
            Text1.Document.Selection.Text = "";
            Clipboard.SetContent(dataPackage);
        }

        private void SizeUp_Click(object sender, RoutedEventArgs e)
        {
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
            Text1.Document.EndUndoGroup();
        }

        private void SizeDown_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            //checks if the font size is too small
            if (Text1.Document.Selection.CharacterFormat.Size > 4)
            {
                //make the selected text font size smaller
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size - 2;
            }
            Text1.Document.EndUndoGroup();
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

        private void CmdBack_Click(object sender, RoutedEventArgs e)
        {
            Settings.Hide();
            SettingsPivot.SelectedIndex = 0; //Set focus to first item in pivot control in the settings panel
        }

        private void Settings_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            SettingsPivot.SelectedItem = SettingsTab1; //Set focus to first item in pivot control in the settings panel
        }

        private void Fonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Text1.Document.BeginUndoGroup();
            if (e.AddedItems[0] is FontFamilyItem selectedFont)
            {
                Text1.Document.Selection.CharacterFormat.Name = selectedFont.Name;
            }
            Text1.Document.EndUndoGroup();
        }

        private void Frame_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //Set text preview in Font Family selector
            var selectedText = Text1.Document.Selection.Text;
            FontFamilyItem.ChangeGlobalPreview(selectedText);
            foreach (var item in AllFonts) item.UpdateLocalPreview();
            //open the font combo box
            Fonts.IsDropDownOpen = true;
        }

        private void FontSelected_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //Change color of the font combo box when hovering over it
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                FontBoxFrame.Background = new SolidColorBrush(Colors.Black);
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                FontBoxFrame.Background = new SolidColorBrush(Colors.White);
            }

            if (this.RequestedTheme == ElementTheme.Dark)
            {
                FontBoxFrame.Background = new SolidColorBrush(Colors.Black);
            }
            else if (this.RequestedTheme == ElementTheme.Light)
            {
                FontBoxFrame.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void FontSelected_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FontBoxFrame.Background = Fonts.Background; //Make the frame over the font box the same color as the font box
        }

        private async void ShowFontsDialog_Click(object sender, RoutedEventArgs e)
        {
            //Save selection point
            int previousPosition = Text1.Document.Selection.StartPosition;
            int previousSelectionEnd = Text1.Document.Selection.EndPosition;
            //Force select all text
            Text1.Focus(FocusState.Programmatic);
            Text1.Document.Selection.SetRange(0, totalCharacters);
            //Get format info about selection
            var selection = Text1.Document.Selection;
            if (selection != null)
            {
                var formatting = selection.CharacterFormat;
                //Update to dialog
                FontAndFormat.FontNameSuggestionInput = formatting.Name;
                FontAndFormat.FontSizeSelection = Convert.ToInt32(formatting.Size);
                FontAndFormat.WantBold = formatting.Bold == FormatEffect.On;
                FontAndFormat.WantItalic = formatting.Italic == FormatEffect.On;
                FontAndFormat.WantUnderline = formatting.Underline != UnderlineType.None;
                FontAndFormat.WantStrikethrough = formatting.Strikethrough == FormatEffect.On;
                FontAndFormat.SelectedColor = formatting.ForegroundColor;
            }
            await FontAndFormat.ShowAsync();
            //Apply setting back if user wanted to
            if (FontAndFormat.FinalResult == DialogResult.Yes)
            {
                var formatting = selection.CharacterFormat;
                formatting.Name = FontAndFormat.FontNameSuggestionInput;
                formatting.Size = FontAndFormat.FontSizeSelection;
                formatting.Bold = FontAndFormat.WantBold ? FormatEffect.On : FormatEffect.Off;
                formatting.Italic = FontAndFormat.WantItalic ? FormatEffect.On : FormatEffect.Off;
                formatting.Underline = FontAndFormat.WantUnderline ? UnderlineType.Single : UnderlineType.None;
                formatting.Strikethrough = FontAndFormat.WantStrikethrough ? FormatEffect.On : FormatEffect.Off;
                formatting.ForegroundColor = FontAndFormat.SelectedColor;
            }
            //Restore to that point like nothing ever happen
            if (previousPosition == previousSelectionEnd)
            {
                //Not select anything
                Text1.Document.Selection.StartPosition = previousPosition;
            }
            else
            {
                //Select like what user used to
                Text1.Document.Selection.SetRange(previousPosition, previousSelectionEnd);
            }
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
            }
        }

        public async void SwitchCompactOverlayMode(bool switching)
        {
            if (switching)
            {
                ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

                Shadow1.Visibility = Visibility.Collapsed;
                CommandBar1.Visibility = Visibility.Collapsed;
                Title.Visibility = Visibility.Collapsed;
                FrameTop.Visibility = Visibility.Collapsed;
                CmdSettings.Visibility = Visibility.Collapsed;
                CmdFocusMode.Visibility = Visibility.Collapsed;
                CmdClassicMode.Visibility = Visibility.Collapsed;
                CommandBar3.Visibility = Visibility.Collapsed;
                CommandBarClassic.Visibility = Visibility.Collapsed;
                Grid.SetRow(CommandBar2, 3);
                CommandBar2.Visibility = Visibility.Visible;
                CommandBar2.HorizontalAlignment = HorizontalAlignment.Stretch;
                //
                row0.Height = new GridLength(0);
                row1.Height = new GridLength(0);

                //make text smaller size if user did not do so on their own and if they did not type anything yet.
                Text1.Document.GetText(TextGetOptions.UseCrlf, out var value);
                if (string.IsNullOrEmpty(value) && Text1.FontSize == 18)
                {
                    Text1.FontSize = 16;
                }

                //Hide Find and Replace dialog if it open
                ShowFindAndReplace = false;

                //log even in app center
                Analytics.TrackEvent("Compact Overlay");
            }
            else
            {
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

                Title.Visibility = Visibility.Visible;
                Shadow1.Visibility = Visibility.Visible;
                CommandBar1.Visibility = Visibility.Visible;
                CommandBar2.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetRow(CommandBar2, 1);
                FrameTop.Visibility = Visibility.Visible;
                CmdSettings.Visibility = Visibility.Visible;
                CmdFocusMode.Visibility = Visibility.Visible;
                CmdClassicMode.Visibility = Visibility.Visible;
                CommandBar3.Visibility = Visibility.Visible;

                row0.Height = new GridLength(1, GridUnitType.Auto);
                row1.Height = new GridLength(1, GridUnitType.Auto);

                if (ClassicModeSwitch == true)
                {
                    SwitchClassicMode(true);
                    CommandBarClassic.Visibility = Visibility.Visible;
                }
            }
        }

        public void SwitchFocusMode(bool switching)
        {
            if (switching)
            {
                Text1.SetValue(Canvas.ZIndexProperty, 90);
                CommandBar1.Visibility = Visibility.Collapsed;
                CommandBar2.Visibility = Visibility.Collapsed;
                Shadow2.Visibility = Visibility.Collapsed;
                Shadow1.Visibility = Visibility.Collapsed;
                row1.Height = new GridLength(0);
                row3.Height = new GridLength(0);
            }
            else
            {
                Text1.SetValue(Canvas.ZIndexProperty, 0);
                CommandBar2.Visibility = Visibility.Visible;
                CommandBar1.Visibility = Visibility.Visible;
                Shadow2.Visibility = Visibility.Visible;
                Shadow1.Visibility = Visibility.Visible;
                CommandBarClassic.Visibility = Visibility.Collapsed;
                row1.Height = new GridLength(1, GridUnitType.Auto);
                row3.Height = new GridLength(1, GridUnitType.Auto);
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
                CommandBar3.Visibility = Visibility.Collapsed;
                Shadow2.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandBar1.Visibility = Visibility.Visible;
                CommandBar2.Visibility = Visibility.Visible;
                CommandBar3.Visibility = Visibility.Visible;
                Shadow2.Visibility = Visibility.Visible;
            }
        }

        //Use for Click function
        public void TurnOnFocusMode() => FocusModeSwitch = true;
        public void TurnOnClassicMode() => ClassicModeSwitch = true;
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
                MaximumPossibleSearchRange = _content.Length;
            }
        }
        /// <summary>
        /// Temporary store the copy of text when it loaded, 
        /// if it didn't match the textbox=it changed
        /// </summary>
        private string initialLoadedContent;

        private void Text1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
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
            FontSelected.Text = Text1.Document.Selection.CharacterFormat.Name; //updates font box to show the selected characters font

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
            totalCharacters = selectedCount;

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
                    }
                }
            }
        }

        async void DelayFocus()
        {
            await Task.Delay(100);
            FindAndReplaceDialog.FindInput.Focus(FocusState.Keyboard);
            FindAndReplaceDialog.FindInput.SelectionStart = FindAndReplaceDialog.FindInput.Text.Length;
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

        private void FindRequestedText(string find, bool direction, bool match, bool wrap)
        {
            if (string.IsNullOrEmpty(find))
            {
                //Nothing to search for
                return;
            }
            if (direction)
            {
                StartSearchPosition = Text1.TextDocument.Selection.FindText(find, MaximumPossibleSearchRange, match ? FindOptions.Case : FindOptions.None);
            }
            else if (!direction)
            {
                int result = 0;
                int backward = Text1.TextDocument.Selection.StartPosition - find.Length;
                if (backward < 1)
                {
                    backward = MaximumPossibleSearchRange;
                }
                while (backward > 1 && result < 1)
                {
                    Text1.TextDocument.Selection.StartPosition = backward;
                    result = Text1.TextDocument.Selection.FindText(find, find.Length + 1, match ? FindOptions.Case : FindOptions.None);
                    backward--;
                    if (backward < 2 && result == 0 && wrap)
                    {                        
                        Text1.TextDocument.Selection.SetRange(MaximumPossibleSearchRange, MaximumPossibleSearchRange);
                        FindRequestedText(find, direction, match, false);
                        break;
                    }
                    else if (backward < 2 && result == 0 && !wrap)
                    {
                        FindRequestedText(find, true, match, false);
                    }
                }
            }

            if (StartSearchPosition < 1 && wrap)
            {
                Text1.TextDocument.Selection.SetRange(0, 0);
                FindRequestedText(find, direction, match, false);
            }
        }

        private void FindAndReplaceRequestedText(string find, string replace, bool direction, bool match, bool wrap, bool all)
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
            if (all)
            {
                //Replace all
                while (true) //Eternity loop
                {
                    //Mark start and end position
                    int start = Text1.TextDocument.Selection.StartPosition;
                    int end = Text1.TextDocument.Selection.EndPosition;
                    //Send find request
                    FindRequestedText(find, direction, match, false);
                    if (Text1.TextDocument.Selection.StartPosition != start &&
                        Text1.TextDocument.Selection.EndPosition != end)
                    {
                        //Found.. Replace
                        Text1.TextDocument.Selection.Text = replace;
                    }
                    else
                    {
                        //It's can't find anymore
                        break;
                    }
                }
            }
            else
            {
                //Mark start and end position
                int start = Text1.TextDocument.Selection.StartPosition;
                int end = Text1.TextDocument.Selection.EndPosition;
                //Send find request
                FindRequestedText(find, direction, match, wrap);
                if (Text1.TextDocument.Selection.StartPosition != start &&
                    Text1.TextDocument.Selection.EndPosition != end)
                {
                    //Found.. Replace
                    Text1.TextDocument.Selection.Text = replace;
                }
            }
        }

        //Menubar function
        public void OpenFindDialogWithReplace()
        {
            FindAndReplaceDialog.ShowReplace = true;
            ShowFindAndReplace = true;
        }

        public async void CMDGoTo_Click()
        {
            Dialog.GoTo line = new Dialog.GoTo()
            {
                RequestedTheme = QSetting.Theme
            };
            await line.ShowAsync();
            if (line.finalResult == DialogResult.Yes)
            {
                Text1.TextDocument.GetText(TextGetOptions.None, out string value);
                totalLine = value.Count(i => i == '\n' || i == '\r');
                totalCharacters = value.Length;
                System.Diagnostics.Debug.WriteLine($"\"{value}\"");
                int input = int.Parse(line.LineInput.Text) - 1;
                if (input < 1) { input = 1; }
                if (input > totalLine)
                {
                    Text1.TextDocument.Selection.StartPosition = totalCharacters;
                    Text1.TextDocument.Selection.EndPosition = totalCharacters;
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
                }
            }
        }
        #endregion
    }

    public class FontColorItem : INotifyPropertyChanged
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

        string _name;
        public string ColorName
        {
            get => _name;
            set => Set(ref _name, value);
        }

        string _tname;
        public string TechnicalName
        {
            get => _tname;
            set => Set(ref _tname, value);
        }

        Color _ac;
        public Color ActualColor
        {
            get => _ac;
            set => Set(ref _ac, value);
        }

        public FontColorItem()
        {
            ColorName = "Default";
            TechnicalName = "Default";
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                ActualColor = Colors.White;
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                ActualColor = Colors.Black;
            }
        }

        public FontColorItem(string name)
        {
            ColorName = ResourceLoader.GetForCurrentView().GetString($"FontColor{name}");
            TechnicalName = name;
            ActualColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), name);
        }
        public FontColorItem(string name, string technical)
        {
            ColorName = ResourceLoader.GetForCurrentView().GetString($"FontColor{name}");
            TechnicalName = technical;
            ActualColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), technical);
        }

        public static FontColorItem Default => new FontColorItem();
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
