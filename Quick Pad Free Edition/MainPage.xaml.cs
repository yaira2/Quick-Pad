using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Quick_Pad_Free_Edition
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        string UpdateFile;
        String FullFilePath; //this is the opened files full path
        string AdRemove; //string indicates if the user paid to remove ads
        private StoreContext context = null;
        string key; //future access list
        private bool _isPageLoaded = false;
        Int64 LastFontSize; //this value is the last selected characters font size
        public System.Timers.Timer timer = new System.Timers.Timer(10000); //this is the auto save timers interval
        public MainPage()
        {
            InitializeComponent();
            //stuff for compact overlay
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(900, 900);

            //Default file name is "New Document"
            //Displays file name on title bar
            UpdateFile = "New Document";
            TQuick.Text = UpdateFile;

            //extent app in to the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            //add all installed fonts to the font box
            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            foreach (string font in fonts)
            {
                Fonts.Items.Add(string.Format(font));
            }

            //lets us know where app setting are
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //check if auto save is on or off
            String launchValue = localSettings.Values["AutoSave"] as string;
            if (launchValue == "On")
            {
                AutoSaveSwitch.IsOn = true; //turn auto save switch on in settings panel.

                //start auto save timer
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(send);
                timer.AutoReset = true;
            }
            else
            {
                AutoSaveSwitch.IsOn = false; //keep auto save switch off in settings panel.
            }

            //call method to check setting if app should be open on top of other windows
            OnTopCheck();

            //get some theme settings in
            String localValue = localSettings.Values["Theme"] as string;

            if (localValue == "Light")
            {
                this.RequestedTheme = ElementTheme.Light;
                Light.IsChecked = true; //select the light theme option in the settings panel

                //log even in app center
                Analytics.TrackEvent("Loaded app in light theme");
            }
            if (localValue == "Dark")
            {
                this.RequestedTheme = ElementTheme.Dark;
                Dark.IsChecked = true; //select the dark theme option in the settings panel

                //log even in app center
                Analytics.TrackEvent("Loaded app in dark theme");
            }
            if (localValue == "System Default")
            {
                this.RequestedTheme = ElementTheme.Default;
                SystemDefault.IsChecked = true; //select the default theme option in the settings panel
            }

            //make the minimize, maximize and close button visible in light theme
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }

            if (this.RequestedTheme == ElementTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else if (this.RequestedTheme == ElementTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }

            //Call method to remove ads for a paid user
            CheckIfPaidForNoAds();

            //check if it is a new user
            String NewUser = localSettings.Values["NewUser"] as string;
            if (NewUser == "1")
            {
                localSettings.Values["NewUser"] = "2"; //third time using the app
                NewUserFeedbackAsync(); //call method that asks user to review the app
            }
            else
            {
                if (NewUser == "0")
                {
                    localSettings.Values["NewUser"] = "1"; //second time using the app
                }
            }
            if (NewUser != "0" && NewUser != "1" && NewUser != "2")
            {
                localSettings.Values["NewUser"] = "0"; //first time using the app
            }

            //ask user if they want to save before closing the app
            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested +=
        async (sender, args) =>
        {
            if (TQuick.Text == UpdateFile)
            {
                //close if file is up to date already
                App.Current.Exit();
            };
            args.Handled = true;

            //popup dialog to ask user if they want to save their work
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Save your work?",
                Content = "Would you like to save your work?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            Settings.Hide(); //close the settings dialog so the app does not hang
            AboutDialog.Hide(); //close the about dialog so the app does not hang

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            //Save file if user clicks yes.
            /// Otherwise, exit without saving.
            if (result == ContentDialogResult.Primary)
            {
                await SaveWork(); //shows save dialog box
                App.Current.Exit(); //then closes
            }
            else
            {
                App.Current.Exit(); //closes without saving
            }
        };

            //check for push notifications
            CheckPushNotifications();

            //code needed to focus on text box on app launch
            this.Loaded += MainPage_Loaded;
            this.LayoutUpdated += MainPage_LayoutUpdated;
        }

        public void send(object source, System.Timers.ElapsedEventArgs e)
        {
            //timer for auto save
            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            async () =>
                {
                    if (TQuick.Text != UpdateFile)
                    {
                        try
                        {
                            //tries to update file if it exsits and is not read only
                            Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                            await PathIO.WriteTextAsync(FullFilePath, value);
                            //update title bar to indicate file is up to date
                            TQuick.Text = UpdateFile;
                        }

                        catch (Exception)
                        {

                        }
                    }
                });
        #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void OnTopCheck()
        {
            //let app know where settings are stored
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //check if the setting is to launch in compact overlay mode
            String launchValue = localSettings.Values["LaunchMode"] as string;

            if (launchValue == "OnTop")
            {
                //launch compact overlay mode
                CompactOverlay.IsChecked = true;
                LaunchModeSwitch.IsOn = true; //toggle the launch compact overlay mode switch in the settings panel.

                //log even in app center
                Analytics.TrackEvent("Loaded app in compact overlay mode");
            }
            else
            {
                LaunchModeSwitch.IsOn = false; //keep launch compact overlay mode switch off in settings panel.
            }
        }

        // Stuff for putting the focus on the content
        private void MainPage_LayoutUpdated(object sender, object e)
        {
            if (_isPageLoaded == true)
            {
                Text1.Focus(FocusState.Programmatic); // Set focus on the main content so the user can start typing right away
                _isPageLoaded = false;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = true;
        }

        public async void CheckPushNotifications()
        {
             //regisiter for push notifications
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

        private StoreContext storeContext = StoreContext.GetDefault();
        
        // Assign this variable to the Store ID of your subscription add-on.
        private string StoreId = "9PMFXLSMJ8RL";
        public async void CheckIfPaidForNoAds()
        {
            StoreAppLicense appLicense = await storeContext.GetAppLicenseAsync();

            // Check if the customer has the rights to the subscription.
            foreach (var addOnLicense in appLicense.AddOnLicenses)
            {
                StoreLicense license = addOnLicense.Value;
                if (license.SkuStoreId.StartsWith(StoreId))
                {
                    if (license.IsActive)
                    {
                        AdRemove = "Paid";
                        Ad1.Visibility = Visibility.Collapsed;
                        RemoveAd.Visibility = Visibility.Collapsed;
                        Text1.Margin = new Thickness(0, 81, 0, 0);
                    }
                }
            }
        }

        public async void NewUserFeedbackAsync()
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Enjoy using Quick Pad?",
                Content = "Would you like to review Quick Pad?",
                PrimaryButtonText = "Review",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                bool doReview = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (args != null)
            {
                if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    string strFilePath = fileArgs.Files[0].Path;
                    var file = (StorageFile)fileArgs.Files[0];
                    await LoadFasFile(file);
                }
            }
        }

        private async Task LoadFasFile(StorageFile file)
        {
            var read = await FileIO.ReadTextAsync(file);
            // Text1.Document.Selection.Text = read;

            UpdateFile = file.DisplayName;
            TQuick.Text = UpdateFile;
            FullFilePath = file.Path;

            Windows.Storage.Streams.IRandomAccessStream randAccStream =
         await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

            // Load the file into the Document property of the RichEditBox.
            Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);

        }

        private async void CmdSettings_Click(object sender, RoutedEventArgs e)
        {
            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("OpenedSettings");

            ContentDialogResult result = await Settings.ShowAsync();
        }

        private void Justify_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = Windows.UI.Text.ParagraphAlignment.Justify;

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Justify Align");
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Right Align");
        }

        private void Center_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Center Align");
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Left;

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Left Align");
        }

        private async void CmdNew_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Save your work?",
                Content = "Would you like to save your work?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                await SaveWork();
                Text1.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, string.Empty);
            }
            else
            {
                Text1.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, string.Empty);
            }
            UpdateFile = "New Document"; //reset the value of the friendly file name
            TQuick.Text = UpdateFile; //update the title bar to reflect it is a new document
            FullFilePath = ""; //clear the path of the open file since there is none

            //log even in app center
            Analytics.TrackEvent("New Document Created");
        }

        private async void CmdOpen_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf"); //add file types that can be opened to the file picker

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    Windows.Storage.Streams.IRandomAccessStream randAccStream =
                await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    UpdateFile = file.DisplayName;
                    TQuick.Text = UpdateFile;
                    FullFilePath = file.Path;

                    key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

                    // Load the file into the Document property of the RichEditBox.
                    Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);

                    //log even in app center
                    Analytics.TrackEvent("Document Opened With File Picker");
                }
                catch (Exception)
                {
                    ContentDialog errorDialog = new ContentDialog()
                    {
                        Title = "File open error",
                        Content = "Sorry, I couldn't open the file.",
                        PrimaryButtonText = "Ok"
                    };

                    await errorDialog.ShowAsync();
                }
            }

        }

        public async Task SaveWork()
        {
                try
                {
                //tries to update file if it exsits and is not read only
                Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                await PathIO.WriteTextAsync(FullFilePath, value);
                //update title bar to indicate file is up to date
                TQuick.Text = UpdateFile;
            }

            catch (Exception)

                {
                    Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

                    savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                    // Dropdown of file types the user can save the file as
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });

                    // Default file name if the user does not type one in or select a file to replace
                    savePicker.SuggestedFileName = UpdateFile;

                    Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        //get the text to save
                        Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);

                        //update title bar
                        UpdateFile = file.DisplayName;
                        TQuick.Text = UpdateFile;
                        FullFilePath = file.Path;

                    key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

                    //write the text to the file
                    await FileIO.WriteTextAsync(file, value);

                    // Let Windows know that we're finished changing the file so the 
                    // other app can update the remote version of the file.
                    Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                        if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                        {
                            //let user know if there was an error saving the file
                            Windows.UI.Popups.MessageDialog errorBox =
                                new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                            await errorBox.ShowAsync();
                        }
                    }
                }
            }

        public async void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            //call the function to save
            await SaveWork();
        }

        private void CmdUndo_Click(object sender, RoutedEventArgs e)
        {
            //undo changes the user did to the text
            Text1.Document.Undo();

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Undo");
        }

        private void CmdRedo_Click(object sender, RoutedEventArgs e)
        {
            //redo changes the user did to the text
            Text1.Document.Redo();

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Redo");
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            //set the selected text to be bold if not already
            //if the text is already bold it will make it regular
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Bold = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Bold");
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            //set the selected text to be in italics if not already
            //if the text is already in italics it will make it regular
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Italic = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Italic");
        }

        private void Underline_Click_1(object sender, RoutedEventArgs e)
        {
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

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Underline");
        }

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                //if there is nothing to paste then dont paste anything since it wil crash
                if (text == "")
                {
                }
                else
                {
                    //paste the text from the clipboard
                    Text1.Document.Selection.TypeText(text);
                }
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Paste");
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            //send the selected text to the clipboard
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text1.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Copy");
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            //deletes the selected text but sends it to the clipboard to be pasted somewhere else
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Text1.Document.Selection.Text);
            Text1.Document.Selection.Text = "";
            Clipboard.SetContent(dataPackage);

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Cut");
        }

        private async void CmdAbout_Click(object sender, RoutedEventArgs e)
        {
            //shows the about dialog
            ContentDialogResult result = await AboutDialog.ShowAsync();

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Opened About");
        }

        private void SizeUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //makes the selected text font size bigger
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size + 2;
            }
            catch (Exception)
            {
                Text1.Document.Selection.CharacterFormat.Size = LastFontSize;
            }
           
            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Size Up");
        }

        private void SizeDown_Click(object sender, RoutedEventArgs e)
        {
            //checks if the font size is too small
            if (Text1.Document.Selection.CharacterFormat.Size > 4)
            {
                //make the selected text font size smaller
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size - 2;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Size Down");
        }

        private async void CompactOverlay_Checked(object sender, RoutedEventArgs e)
        {
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            //compactOptions.CustomSize = new Windows.Foundation.Size(426, 300);
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(450, 200));

            //specific to my app 
            Grid.SetRow(CommandBar2, 2);
            Shadow1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CommandBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Title.Visibility = Visibility.Collapsed;
            CommandBar2.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            FrameTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Text1.Margin = new Thickness(0, 0, 0, 0);
            CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CommandBar3.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Ad1.Visibility = Visibility.Collapsed;
            CommandBar2.Margin = new Thickness(0, 0, 0, 0);
            TQuick.Visibility = Visibility.Collapsed;

            //make text smaller size if user did not do so on their own and if they did not type anything yet.
            Text1.Document.GetText(TextGetOptions.UseCrlf, out var value);
            if (string.IsNullOrEmpty(value) && Text1.FontSize == 24)
            {
                Text1.FontSize = 18;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Compact Overlay");

            //log even in app center
            Analytics.TrackEvent("Compact Overlay");
        }

        private async void CompactOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            if (AdRemove == "Paid")
            {
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                Grid.SetRow(CommandBar2, 0);
                Title.Visibility = Visibility.Visible;
                Shadow1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar2.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                FrameTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Text1.Margin = new Thickness(0, 81, 0, 0);
                CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar3.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar2.Margin = new Thickness(0, 33, 0, 0);
                TQuick.Visibility = Visibility.Visible;
            }
            else
            {
                bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                Grid.SetRow(CommandBar2, 0);
                Title.Visibility = Visibility.Visible;
                Shadow1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar2.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                FrameTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Text1.Margin = new Thickness(0, 81, 0, 90);
                CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar3.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Ad1.Visibility = Visibility.Visible;
                CommandBar2.Margin = new Thickness(0, 33, 0, 0);
                TQuick.Visibility = Visibility.Visible;
            }
        }

        private void Emoji_Checked(object sender, RoutedEventArgs e)
        {
            Emoji2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            E1.Focus(FocusState.Programmatic);

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("EmojiPanel");

            //log even in app center
            Analytics.TrackEvent("User opened emoji panel");
        }

        private void Emoji_Unchecked(object sender, RoutedEventArgs e)
        {
            Emoji2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void EmojiPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void Fonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFont = e.AddedItems[0].ToString();
            Text1.Document.Selection.CharacterFormat.Name = selectedFont;
        }
        
        public void EmojiSub(object sender, RoutedEventArgs e)
        {
            string objname = ((Button)sender).Content.ToString(); //get emoji from button that was pressed
            Text1.Document.Selection.TypeText(objname); //add it to the text box
            TQuick.Text = "*" + UpdateFile; //add star to title bar to indicate unsaved file

            //log even in app center
            Analytics.TrackEvent("User inserted an emoji");
        }

        private void TextColor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Yellow_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.LightYellow);
        }

        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.SkyBlue);
        }

        private void Pink_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.LightPink);
        }

        private void Orange_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.LightSalmon);
        }

        private void Green_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.LightGreen);
        }

        private void Black_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.Black);
        }

        private async void NewWindows_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(MainPage), null);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

        }

        private void Text1_GotFocus(object sender, RoutedEventArgs e)
        {
            Emoji.IsChecked = false; //hide emoji panel if open 
            LastFontSize = Convert.ToInt64(Text1.Document.Selection.CharacterFormat.Size); //get font size of last selected character
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {

        }

        private void CmdShare_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Shared Text");
        }

        void MainPage_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
        {
            Text1.Document.GetText(TextGetOptions.UseCrlf, out var value);

            if (!string.IsNullOrEmpty(value))
            {
                args.Request.Data.SetText(value);
                args.Request.Data.Properties.Title = Windows.ApplicationModel.Package.Current.DisplayName;
            }
            else
            {
                args.Request.FailWithDisplayText("Nothing to share, type something in order to share it.");
            }
        }

        private void Text1_TextChanged(object sender, RoutedEventArgs e)
        {
            if (Text1.Document.CanUndo() == true)
            {
                CmdUndo.IsEnabled = true;
            }
            else
            {
                CmdUndo.IsEnabled = false;
            }
            /////
            if (Text1.Document.CanRedo() == true)
            {
                CmdRedo.IsEnabled = true;
            }
            else
            {
                CmdRedo.IsEnabled = false;
            }
        }

        private async void CmdReview_Click(object sender, RoutedEventArgs e)
        {
            bool doReview = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));

            //log even in app center
            Analytics.TrackEvent("User clicked on review");
        }

        private async void RemoveAd_Click(object sender, RoutedEventArgs e)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
            }

            StorePurchaseResult result = await context.RequestPurchaseAsync("9PMFXLSMJ8RL");

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    AdRemove = "Paid";
                    Ad1.Visibility = Visibility.Collapsed;
                    RemoveAd.Visibility = Visibility.Collapsed;
                    Text1.Margin = new Thickness(0, 81, 0, 0);
                    break;

                case StorePurchaseStatus.Succeeded:
                    AdRemove = "Paid";
                    Ad1.Visibility = Visibility.Collapsed;
                    RemoveAd.Visibility = Visibility.Collapsed;
                    Text1.Margin = new Thickness(0, 81, 0, 0);

                    var dialog = new MessageDialog("The purchase was successful, thank you for being a Quick Pad user!");
                    await dialog.ShowAsync();
                    break;

                default:
                    var Erordialog = new MessageDialog("The purchase was unsuccessful due to an unknown error.");
                    await Erordialog.ShowAsync();
                    break;
            }

            //log even in app center
            Analytics.TrackEvent("User pressed remove ads");
        }

        private void Strikethrough_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Text.ITextSelection selectedText = Text1.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Strikethrough = Windows.UI.Text.FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Strikethrough");
        }

        private void BulletList_Click(object sender, RoutedEventArgs e)
        {
            if (Text1.Document.Selection.ParagraphFormat.ListType == MarkerType.Bullet)
            {
                Text1.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
            }
            else
            {
                Text1.Document.Selection.ParagraphFormat.ListType = MarkerType.Bullet;
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Bullet List");
        }

        private void White_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.White);
        }

        private void CmdBack_Click(object sender, RoutedEventArgs e)
        {
            Settings.Hide();
        }

        private void Light_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Light";
            this.RequestedTheme = ElementTheme.Light;

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.Black;
        }

        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Dark";
            this.RequestedTheme = ElementTheme.Dark;

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.White;
        }

        private void SystemDefault_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "System Default";
            this.RequestedTheme = ElementTheme.Default;

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }

            if (this.RequestedTheme == ElementTheme.Dark)
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            else if (this.RequestedTheme == ElementTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }
        }

        private void LaunchModeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["LaunchMode"] = "OnTop";
                }
                else
                {
                    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["LaunchMode"] = "Regular";
                }
            }

            //Let me know know user used this feature
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log("Open On Top Setting");
        }

        private void Settings_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {

        }

        private void AboutDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {

        }

        private void Text1_GotFocus_1(object sender, RoutedEventArgs e)
        {

        }

        private void Text1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TQuick.Text = "*" + UpdateFile; //add star to title bar to indicate unsaved file
        }

        private async void Feedback_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();

            //log even in app center
            Analytics.TrackEvent("User pressed feedback");
        }

        private void AutoSaveSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["AutoSave"] = "On";
                }
                else
                {
                    ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localSettings.Values["AutoSave"] = "Off";
                }
            }
        }
    }
}
