using Microsoft.AppCenter.Analytics;
using Microsoft.Services.Store.Engagement;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Quick_Pad_Free_Edition
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string UpdateFile = "New Document"; //Default file name is "New Document"
        private String FullFilePath; //this is the opened files full path
        private string AdRemove; //string indicates if the user paid to remove ads
        private StoreContext context = null;
        private string key; //future access list
        private bool _isPageLoaded = false;
        private Int64 LastFontSize; //this value is the last selected characters font size
        private String SaveDialogValue;
        private string DefaultFileExt;
        public System.Timers.Timer timer = new System.Timers.Timer(10000); //this is the auto save timers interval
        public MainPage()
        {
            InitializeComponent();

            TQuick.Text = UpdateFile; //Displays file name on title bar

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
                DefaultFont.Items.Add(string.Format(font));
            }

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //lets us know where app setting are

            DefaultFileExt = localSettings.Values["DefaultFileType"] as string; //get the default file type
            if (DefaultFileExt == ".txt")
            {
                DefaultFileType.SelectedValue = ".txt";
            }

            //check if auto save is on or off
            String launchValue = localSettings.Values["AutoSave"] as string;
            if (launchValue == "Off")
            {
                AutoSaveSwitch.IsOn = false; //keep auto save switch off in settings panel.
            }
            else
            {
                AutoSaveSwitch.IsOn = true; //turn auto save switch on in settings panel.

                //start auto save timer
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(send);
                timer.AutoReset = true;
            }

            //check if word wrap is on or off
            String WordWrapSetting = localSettings.Values["WordWrap"] as string;
            if (WordWrapSetting == "No")
            {
                WordWrap.IsOn = false; //keep word wrap switch off in settings panel.
                Text1.TextWrapping = TextWrapping.NoWrap; //turn off word wrap
            }
            else
            {
                WordWrap.IsOn = true; //turn word wrap switch on in settings panel.
                Text1.TextWrapping = TextWrapping.Wrap; //turn on word wrap
            }

            //check if spell check is on or off
            String spellchecksetting = localSettings.Values["SpellCheck"] as string;
            if (spellchecksetting == "No")
            {
                SpellCheck.IsOn = false; //keep spell check switch off in settings panel.
                Text1.IsSpellCheckEnabled = false; //turn spell check off
            }
            else
            {
                SpellCheck.IsOn = true; //turn spell check switch on in settings panel.
                Text1.IsSpellCheckEnabled = true; //turn spell on
            }

            CheckToolbarOptions(); //check which buttons to show in toolbar
            CheckIfPaidForNoAds(); //Call method to remove ads for a paid user
            CheckTheme(); //check the theme

            VersionNumber.Text = string.Format("Version: {0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

            //check how many times the app was run
            String NewUser = localSettings.Values["NewUser"] as string;
            if (NewUser == "1") //second time using the app
            {
                localSettings.Values["NewUser"] = "2";
                NewUserFeedbackAsync(); //call method that asks user if they want to review the app
            }
            if (NewUser != "1" && NewUser != "2") //first time using the app
            {
                localSettings.Values["NewUser"] = "1";
            }

            //check if focus is on app or off the app
            Window.Current.CoreWindow.Activated += (sender, args) =>
            {
                if (args.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                {
                    CommandBar2.Focus(FocusState.Programmatic); // Set focus off the main content
                }
            };

            //ask user if they want to save before closing the app
            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested +=
        async (sender, args) =>
        {
            if (TQuick.Text == UpdateFile)
            {
                App.Current.Exit();  //close if file is up to date already

            };
            args.Handled = true;

            SaveDialog.Hide();
            Settings.Hide(); //close the settings dialog so the app does not hang

            await SaveDialog.ShowAsync();

            if (SaveDialogValue != "Cancel")
            {
                 App.Current.Exit();
            }

            SaveDialogValue = ""; //reset save dialog 
           
        };

            CheckPushNotifications(); //check for push notifications

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
                        var result = FullFilePath.Substring(FullFilePath.Length - 4); //find out the file extension
                        if ((result.ToLower() != ".rtf"))
                        {
                            //tries to update file if it exsits and is not read only
                            Text1.Document.GetText(TextGetOptions.None, out var value);
                            await PathIO.WriteTextAsync(FullFilePath, value);
                            TQuick.Text = UpdateFile; //update title bar to indicate file is up to date
                        }
                        if (result.ToLower() == ".rtf")
                        {
                            //tries to update file if it exsits and is not read only
                            Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                            await PathIO.WriteTextAsync(FullFilePath, value);
                            TQuick.Text = UpdateFile; //update title bar to indicate file is up to date
                        }
                    }

                    catch (Exception) { }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void LaunchCheck()
        {
            //check what mode to launch the app in

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //let app know where settings are stored
            String launchValue = localSettings.Values["LaunchMode"] as string;

            if (launchValue == "On Top")
            {
                CompactOverlay.IsChecked = true; //launch compact overlay mode
                LaunchOptions.SelectedValue = "On Top";
            }

            if (launchValue == "Focus Mode")
            {
                SwitchToFocusMode();
                LaunchOptions.SelectedValue = "Focus Mode";
            }

            if (launchValue == "Default")
            {
                LaunchOptions.SelectedValue = "Default";
            }
        }

        //check the theme
        private void CheckTheme()
        {
            //get some theme settings in
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //lets us know where app setting are

            String localValue = localSettings.Values["Theme"] as string;
            if (localValue == "Light") //light theme is on
            {
                this.RequestedTheme = ElementTheme.Light;
                Light.IsChecked = true; //select the light theme option in the settings panel
                Analytics.TrackEvent("Loaded app in light theme");  //log even in app center
            }
            if (localValue == "Dark") //dark theme is on
            {
                this.RequestedTheme = ElementTheme.Dark;
                Dark.IsChecked = true; //select the dark theme option in the settings panel

                Analytics.TrackEvent("Loaded app in dark theme"); //log even in app center
            }
            if (localValue == "System Default") //default theme is on
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
        }

        //check which buttons to show in toolbar
        private void CheckToolbarOptions()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //let app know where settings are stored

            //check if the share option should show
            String ShowS = localSettings.Values["ShowShare"] as string;

            if (ShowS == "No")
            {
                ShowShare.IsOn = false; //toggle the show share option in the settings panel.
                CmdShare.Visibility = Visibility.Collapsed; //hide share button
            }
            else
            {
                ShowShare.IsOn = true; //toggle the show share option in the settings panel.
            }

            //check if the bullet list option should show
            String ShowBulletsSetting = localSettings.Values["ShowBullets"] as string;

            if (ShowBulletsSetting == "No")
            {
                ShowBullets.IsOn = false; //toggle the show bullets option in the settings panel.
                BulletList.Visibility = Visibility.Collapsed; //hid bullet option
            }
            else
            {
                ShowBullets.IsOn = true; //toggle the show bullets option in the settings panel.
            }

            //check if strikethrough option should show
            String ShowST = localSettings.Values["ShowStrikethroughOption"] as string;

            if (ShowST == "No")
            {
                //hide ShowStrikethroughOption
                ShowStrikethrough.IsOn = false; //toggle the show ShowStrikethroughOption option in the settings panel.
                Strikethrough.Visibility = Visibility.Collapsed; //hide strikethrough option
            }
            else
            {
                ShowStrikethrough.IsOn = true; //toggle the show ShowStrikethroughOption option in the settings panel.
            }

            //check if the left align option should show
            String ShowAl = localSettings.Values["ShowAlignLeft"] as string;

            if (ShowAl == "No")
            {
                //hide option
                ShowAlignLeft.IsOn = false; //toggle the option in the settings panel.
                Left.Visibility = Visibility.Collapsed; //hide the button
            }
            else
            {
                ShowAlignLeft.IsOn = true; //toggle the option in the settings panel.
            }

            //check if the center align option should show
            String ShowAC = localSettings.Values["ShowAlignCenter"] as string;

            if (ShowAC == "No")
            {
                //hide option
                ShowAlignCenter.IsOn = false; //toggle the option in the settings panel.
                Center.Visibility = Visibility.Collapsed; //hide the button
            }
            else
            {
                ShowAlignCenter.IsOn = true; //toggle the option in the settings panel.
            }

            //check if the right align option should show
            String ShowAR = localSettings.Values["ShowAlignRight"] as string;

            if (ShowAR == "No")
            {
                //hide option
                ShowAlignRight.IsOn = false; //toggle the option in the settings panel.
                Right.Visibility = Visibility.Collapsed; //hide the button
            }
            else
            {
                ShowAlignRight.IsOn = true; //toggle the option in the settings panel.
            }

            //check if the justify align option should show
            String ShowAJ = localSettings.Values["ShowAlignJustify"] as string;

            if (ShowAJ == "No")
            {
                //hide option
                ShowAlignJustify.IsOn = false; //toggle the option in the settings panel.
                Justify.Visibility = Visibility.Collapsed; //hide the button
            }
            else
            {
                ShowAlignJustify.IsOn = true; //toggle the option in the settings panel.
            }

            if (ShowAl == "No" && ShowAC == "No" && ShowAR == "No" && ShowAJ == "No")
            {
                AlignSeparator.Visibility = Visibility.Collapsed; //hide the separator if all the allignment buttons are hidden
            }
        }

        // Stuff for putting the focus on the content
        private void MainPage_LayoutUpdated(object sender, object e)
        {
            if (_isPageLoaded == true)
            {
                Text1.Focus(FocusState.Programmatic); // Set focus on the main content so the user can start typing right away

                //let app know where setting are saved
                ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //lets us know where app setting are

                //check what default font is
                try
                {
                    String DefaultFonts = localSettings.Values["DefaultFont"] as string;
                    if (DefaultFonts != "Segoe UI")
                    {
                        DefaultFont.PlaceholderText = DefaultFonts;
                        Fonts.PlaceholderText = DefaultFonts;
                        Fonts.SelectedItem = DefaultFonts;
                        FontSelected.Text = Convert.ToString(Fonts.SelectedItem);
                        Text1.Document.Selection.CharacterFormat.Name = DefaultFonts;
                    }
                }
                catch (Exception)
                {
                    localSettings.Values["DefaultFont"] = "Segoe UI"; //set the default font size to Segoe UI
                }

                //check what default font color is
                try
                {
                    String DefaultFontColors = localSettings.Values["DefaultFontColor"] as string;
                    if (DefaultFontColors != "")
                    {
                        DefaultFontColor.SelectedItem = DefaultFontColors;
                        Text1.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), DefaultFontColor.SelectedValue);
                    }
                }
                catch (Exception) //no setting was found
                {
                    if (this.RequestedTheme == ElementTheme.Dark || App.Current.RequestedTheme == ApplicationTheme.Dark)
                    {
                        DefaultFontColor.PlaceholderText = "White";
                    }
                }

                //check what default font size is and set it
                Int16 DefaultFontSizes = Convert.ToInt16(localSettings.Values["DefaultFontSize"]); //load the defualt font size

                if (DefaultFontSizes == 0)
                {
                    localSettings.Values["DefaultFontSize"] = "18"; //set 18 as defualt font size
                    Text1.Document.Selection.CharacterFormat.Size = 18; //set the font size
                }
                else
                {
                    DefaultFontSize.PlaceholderText = Convert.ToString(DefaultFontSizes); //set the selected font size placeholder text in settings to whatever the font size is meant to be
                    Text1.Document.Selection.CharacterFormat.Size = DefaultFontSizes; //set the font size
                }

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
        private string StoreId = "9PMFXLSMJ8RL"; // Assign this variable to the Store ID of your subscription add-on.

        public async void CheckIfPaidForNoAds()
        {
            StoreAppLicense appLicense = await storeContext.GetAppLicenseAsync();
            // Check if the customer has the rights to the subscription, in this case it is not a subscription but a one time iap to remove ads
            foreach (var addOnLicense in appLicense.AddOnLicenses)
            {
                StoreLicense license = addOnLicense.Value;
                if (license.SkuStoreId.StartsWith(StoreId))
                {
                    if (license.IsActive)
                    {
                        AdRemove = "Paid"; //set a value that shows the user paid to remove ads thhat can be used at other times in the app                   
                        RemoveAds(); //remove ads
                    }
                }
            }
            LaunchCheck(); //call method to check what mode the app should launch in
        }

        public void RemoveAds() //hide ads
        {
            Ad1.Suspend();
            Ad1.Visibility = Visibility.Collapsed; //hide the ad
            Ad.Visibility = Visibility.Collapsed;
            RemoveAd.Visibility = Visibility.Collapsed; //hide the remove ad button since they already paid
            Text1.Margin = new Thickness(0, 74, 0, 40); //fix text margin to take up space where ad used to be
        }

        public async void NewUserFeedbackAsync()
        {
            ContentDialog deleteFileDialog = new ContentDialog //brings up a content dialog
            {
                Title = "Do you enjoy using Quick Pad?",
                Content = "Please leave a review for Quick Pad in the Microsoft Store.",
                PrimaryButtonText = "Review",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync(); //get the results if the user clicked to review or not

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                await ShowRatingReviewDialog(); //show the review dialog.

                //log even in app center
                Analytics.TrackEvent("Pressed review from popup in app");
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
                    await LoadFasFile(file); //call method to open the file the app was launched from
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
            SetTaskBarTitle(); //update the title in the taskbar

            Windows.Storage.Streams.IRandomAccessStream randAccStream =
         await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

            // Load the file into the Document property of the RichEditBox.
            if ((file.FileType.ToLower() != ".rtf"))
            {
                Text1.Document.SetText(Windows.UI.Text.TextSetOptions.None, await FileIO.ReadTextAsync(file));
            }
            if (file.FileType.ToLower() == ".rtf")
            {
                Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
            }
        }

        private void SetTaskBarTitle()
        {
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = UpdateFile;
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
            if (TQuick.Text != UpdateFile)
            {
                await SaveDialog.ShowAsync();

                if (SaveDialogValue != "Cancel")
                {
                    Text1.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, string.Empty);

                    UpdateFile = "New Document"; //reset the value of the friendly file name
                    TQuick.Text = UpdateFile; //update the title bar to reflect it is a new document
                    FullFilePath = ""; //clear the path of the open file since there is none
                    SetTaskBarTitle(); //update the title in the taskbar

                    //log even in app center
                    Analytics.TrackEvent("New Document Created");
                }

                SaveDialogValue = ""; //reset save dialog 
            }
            else
            {
                Text1.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, string.Empty);

                UpdateFile = "New Document"; //reset the value of the friendly file name
                TQuick.Text = UpdateFile; //update the title bar to reflect it is a new document
                FullFilePath = ""; //clear the path of the open file since there is none
                SetTaskBarTitle(); //update the title in the taskbar

                //log even in app center
                Analytics.TrackEvent("New Document Created");
            }
        }

        private async void CmdOpen_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf"); //add file types that can be opened to the file picker
            open.FileTypeFilter.Add(".txt"); //add file types that can be opened to the file picker
            open.FileTypeFilter.Add("*"); //add file types that can be opened to the file picker

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
                    SetTaskBarTitle(); //update the title in the taskbar

                    key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file); //let file be accessed later

                    // Load the file into the Document property of the RichEditBox.
                    if ((file.FileType.ToLower() != ".rtf"))
                    {
                        Text1.Document.SetText(Windows.UI.Text.TextSetOptions.None, await FileIO.ReadTextAsync(file));
                    }
                    if (file.FileType.ToLower() == ".rtf")
                    {
                        Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
                    }

                    Analytics.TrackEvent("Document Opened With File Picker"); //log even in app center
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

        public async Task SaveWork()
        {
            try
            {
                var result = FullFilePath.Substring(FullFilePath.Length - 4); //find out the file extension

                if ((result.ToLower() != ".rtf"))
                {
                    //tries to update file if it exsits and is not read only
                    Text1.Document.GetText(TextGetOptions.None, out var value);
                    await PathIO.WriteTextAsync(FullFilePath, value);
                    //update title bar to indicate file is up to date
                    TQuick.Text = UpdateFile;
                }
                if (result.ToLower() == ".rtf")
                {
                    //tries to update file if it exsits and is not read only
                    Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);
                    await PathIO.WriteTextAsync(FullFilePath, value);
                    //update title bar to indicate file is up to date
                    TQuick.Text = UpdateFile;
                }
            }

            catch (Exception)

            {
                Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
                savePicker.FileTypeChoices.Add("All Files", new List<string>() { "." });

                //check if default file type is .txt
                if (DefaultFileExt == ".txt")
                {
                    savePicker.FileTypeChoices.Clear();
                    savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
                    savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                    savePicker.FileTypeChoices.Add("All Files", new List<string>() { "." });
                }

                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = UpdateFile;

                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    //update title bar
                    UpdateFile = file.DisplayName;
                    TQuick.Text = UpdateFile;
                    FullFilePath = file.Path;
                    SetTaskBarTitle(); //update the title in the taskbar

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
                        //get the text to save
                        Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);

                        //write the text to the file
                        await FileIO.WriteTextAsync(file, value);
                    }

                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
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
            await SaveWork(); //call the function to save
        }

        private void CmdUndo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Undo(); //undo changes the user did to the text
        }

        private void CmdRedo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Redo(); //redo changes the user did to the text
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
        }

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                //if there is nothing to paste then dont paste anything since it wil crash
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
            try
            {
                //makes the selected text font size bigger
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size + 2;
            }
            catch (Exception)
            {
                Text1.Document.Selection.CharacterFormat.Size = LastFontSize;
            }
        }

        private void SizeDown_Click(object sender, RoutedEventArgs e)
        {
            //checks if the font size is too small
            if (Text1.Document.Selection.CharacterFormat.Size > 4)
            {
                //make the selected text font size smaller
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size - 2;
            }
        }

        private async void CompactOverlay_Checked(object sender, RoutedEventArgs e)
        {
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            //compactOptions.CustomSize = new Windows.Foundation.Size(426, 300);
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(450, 200));

            Grid.SetRow(CommandBar2, 2);
            Shadow1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CommandBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Title.Visibility = Visibility.Collapsed;
            CommandBar2.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            FrameTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Text1.Margin = new Thickness(0, 0, 0, 0);
            CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CmdFocusMode.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            CommandBar3.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Ad1.Suspend();
            Ad1.Visibility = Visibility.Collapsed;
            Ad.Visibility = Visibility.Collapsed;
            CommandBar2.Margin = new Thickness(0, 0, 0, 0);
            TQuick.Visibility = Visibility.Collapsed;

            //make text smaller size if user did not do so on their own and if they did not type anything yet.
            Text1.Document.GetText(TextGetOptions.UseCrlf, out var value);
            if (string.IsNullOrEmpty(value) && Text1.FontSize == 18)
            {
                Text1.FontSize = 16;
            }

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
                Text1.Margin = new Thickness(0, 74, 0, 40);
                CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CmdFocusMode.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                Text1.Margin = new Thickness(0, 74, 0, 130);
                CmdSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CmdFocusMode.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBar3.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Ad1.Visibility = Visibility.Visible;
                Ad1.Resume();
                Ad.Visibility = Visibility.Visible;
                CommandBar2.Margin = new Thickness(0, 33, 0, 0);
                TQuick.Visibility = Visibility.Visible;
            }
        }

        private void Emoji_Checked(object sender, RoutedEventArgs e)
        {
            Emoji2.Visibility = Windows.UI.Xaml.Visibility.Visible;
            E1.Focus(FocusState.Programmatic);

            //log even in app center
            Analytics.TrackEvent("User opened emoji panel");
        }

        private void Emoji_Unchecked(object sender, RoutedEventArgs e)
        {
            Emoji2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            EmojiPivot.SelectedIndex = 0; //Set focus to first item in pivot control in the emoji panel
        }

        private void EmojiPanel_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        public void EmojiSub(object sender, RoutedEventArgs e)
        {
            string objname = ((Button)sender).Content.ToString(); //get emoji from button that was pressed
            Text1.Document.Selection.TypeText(objname); //add it to the text box
            TQuick.Text = "*" + UpdateFile; //add star to title bar to indicate unsaved file

            //log even in app center
            Analytics.TrackEvent("User inserted an emoji");
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

        private void CmdShare_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;

            //log even in app center
            Analytics.TrackEvent("Shared file");
        }

        private void MainPage_DataRequested(Windows.ApplicationModel.DataTransfer.DataTransferManager sender, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs args)
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

        private async void CmdReview_Click(object sender, RoutedEventArgs e)
        {
            await ShowRatingReviewDialog(); //show the review dialog.

            Analytics.TrackEvent("User clicked on review"); //log even in app center
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
                    Ad1.Suspend();
                    Ad1.Visibility = Visibility.Collapsed;
                    Ad.Visibility = Visibility.Collapsed;
                    RemoveAd.Visibility = Visibility.Collapsed;
                    Text1.Margin = new Thickness(0, 74, 0, 40);
                    break;

                case StorePurchaseStatus.Succeeded:
                    AdRemove = "Paid";
                    Ad1.Suspend();
                    Ad1.Visibility = Visibility.Collapsed;
                    Ad.Visibility = Visibility.Collapsed;
                    RemoveAd.Visibility = Visibility.Collapsed;
                    Text1.Margin = new Thickness(0, 74, 0, 40);

                    Analytics.TrackEvent("User removed ad"); //log even in app center

                    var dialog = new MessageDialog("The purchase was successful, thank you for being a Quick Pad user!");
                    await dialog.ShowAsync();
                    break;

                default:
                    var Erordialog = new MessageDialog("The purchase was unsuccessful due to an unknown error.");
                    await Erordialog.ShowAsync();
                    break;
            }

            Analytics.TrackEvent("User pressed remove ads"); //log even in app center
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
        }

        private void White_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Windows.UI.Colors.White);
        }

        private void CmdBack_Click(object sender, RoutedEventArgs e)
        {
            Settings.Hide();
            SettingsPivot.SelectedIndex = 0; //Set focus to first item in pivot control in the settings panel
        }

        private void Light_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Light";
            this.RequestedTheme = ElementTheme.Light;

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.Black;

            FontBoxFrame.Background = Fonts.Background; //Make the frame over the font box the same color as the font box
        }

        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Dark";
            this.RequestedTheme = ElementTheme.Dark;

            //Make the minimize, maxamize and close button visible
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.White;
            FontBoxFrame.Background = Fonts.Background; //Make the frame over the font box the same color as the font box
        }

        private void SystemDefault_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "System Default";
            RequestedTheme = ElementTheme.Default;
            CheckTheme(); //update the theme
            FontBoxFrame.Background = Fonts.Background; //Make the frame over the font box the same color as the font box
        }

        private void Settings_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            SettingsPivot.SelectedItem = SettingsTab1; //Set focus to first item in pivot control in the settings panel
        }

        private void Text1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TQuick.Text = "*" + UpdateFile; //add star to title bar to indicate unsaved file

            if (e.Key == VirtualKey.Tab)
            {
                RichEditBox richEditBox = sender as RichEditBox;
                if (richEditBox != null)
                {
                    richEditBox.Document.Selection.TypeText("\t");
                    e.Handled = true;
                }
            }
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

        private void ShowBullets_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowBullets"] = "Yes";
                    BulletList.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowBullets"] = "No";
                    BulletList.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowStrikethrough_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowStrikethroughOption"] = "Yes";
                    Strikethrough.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowStrikethroughOption"] = "No";
                    Strikethrough.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ShowShare_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowShare"] = "Yes";
                    CmdShare.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowShare"] = "No";
                    CmdShare.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Text1_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Text1_Drop(object sender, DragEventArgs e)
        {
            //Check if file is open and ask user if they want to save it when dragging a file in to Quick Pad.
            if (TQuick.Text != UpdateFile)
            {
                await SaveDialog.ShowAsync();
                if (SaveDialogValue=="Cancel")
                {
                    SaveDialogValue = ""; //reset save dialog 
                    return;
                }
            }

            //load rich text files droped in from file explorer
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        var storageFile = items[0] as StorageFile;
                        var read = await FileIO.ReadTextAsync(storageFile);
                        // Text1.Document.Selection.Text = read;

                        Windows.Storage.Streams.IRandomAccessStream randAccStream =
                     await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);

                        key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile); //let file be accessed later

                        if ((storageFile.FileType.ToLower() != ".rtf"))
                        {
                            UpdateFile = storageFile.DisplayName;
                            TQuick.Text = UpdateFile;
                            FullFilePath = storageFile.Path;
                            SetTaskBarTitle(); //update the title in the taskbar

                            Text1.Document.SetText(Windows.UI.Text.TextSetOptions.None, await FileIO.ReadTextAsync(storageFile));
                        }

                        if (storageFile.FileType.ToLower() == ".rtf")
                        {
                            UpdateFile = storageFile.DisplayName;
                            TQuick.Text = UpdateFile;
                            FullFilePath = storageFile.Path;
                            SetTaskBarTitle(); //update the title in the taskbar

                            Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
                        }

                        //log even in app center
                        Analytics.TrackEvent("Droped file in to Quick Pad");
                    }
                }
            }
            catch (Exception) { }
        }

        private void WordWrap_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["WordWrap"] = "Yes";
                    Text1.TextWrapping = TextWrapping.Wrap;
                }
                else
                {
                    localSettings.Values["WordWrap"] = "No";
                    Text1.TextWrapping = TextWrapping.NoWrap;
                }
            }
        }

        private void ShowAlignLeft_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowAlignLeft"] = "Yes";
                    Left.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowAlignLeft"] = "No";
                    Left.Visibility = Visibility.Collapsed;
                }
            }

            AlignCheck();
        }

        private void ShowAlignCenter_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowAlignCenter"] = "Yes";
                    Center.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowAlignCenter"] = "No";
                    Center.Visibility = Visibility.Collapsed;
                }
            }

            AlignCheck();
        }

        private void ShowAlignRight_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowAlignRight"] = "Yes";
                    Right.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowAlignRight"] = "No";
                    Right.Visibility = Visibility.Collapsed;
                }
            }

            AlignCheck();
        }

        private void ShowAlignJustify_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["ShowAlignJustify"] = "Yes";
                    Justify.Visibility = Visibility.Visible;
                }
                else
                {
                    localSettings.Values["ShowAlignJustify"] = "No";
                    Justify.Visibility = Visibility.Collapsed;
                }
            }

            AlignCheck();
        }

        private void Text1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            FontSelected.Text = Text1.Document.Selection.CharacterFormat.Name; //updates font box to show the selected characters font
        }

        public void AlignCheck()
        {
            if (Left.Visibility == Visibility.Collapsed && Center.Visibility == Visibility.Collapsed && Right.Visibility == Visibility.Collapsed && Justify.Visibility == Visibility.Collapsed)
            {
                AlignSeparator.Visibility = Visibility.Collapsed; //hide the separator if all the allignment buttons are hidden
            }
            else
            {
                AlignSeparator.Visibility = Visibility.Visible; //Show the separator if not all the allignment buttons are hidden
            }
        }

        private void Fonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFont = e.AddedItems[0].ToString();
            Text1.Document.Selection.CharacterFormat.Name = selectedFont;
        }

        private void Frame_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Fonts.IsDropDownOpen = true; //open the font combo box
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

        private void SpellCheck_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    localSettings.Values["SpellCheck"] = "Yes";
                    Text1.IsSpellCheckEnabled = true;
                }
                else
                {
                    localSettings.Values["SpellCheck"] = "No";
                    Text1.IsSpellCheckEnabled = false;
                }
            }
        }

        private void DefaultFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFont = e.AddedItems[0].ToString();

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultFont"] = selectedFont;

            Fonts.SelectedItem = selectedFont; //make the change take affect right away
        }

        private void DefaultFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFontSize = e.AddedItems[0];

            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultFontSize"] = selectedFontSize;

            Text1.Document.Selection.CharacterFormat.Size = Convert.ToInt64(selectedFontSize); //make the change take affect right away
        }

        private async void SaveDialogYes_Click(object sender, RoutedEventArgs e)
        {
            await SaveWork();
            SaveDialog.Hide();
        }

        private void SaveDialogNo_Click(object sender, RoutedEventArgs e)
        {
            SaveDialog.Hide();
        }

        private void SaveDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            SaveDialogValue = "Cancel";
            SaveDialog.Hide();
        }

        private void DefaultFontColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultFontColor"] = DefaultFontColor.SelectedValue;

            Text1.Document.Selection.CharacterFormat.ForegroundColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), DefaultFontColor.SelectedValue);          
        }

        private void SwitchToFocusMode()
        {
            Text1.SetValue(Canvas.ZIndexProperty, 90);
            Text1.Margin = new Thickness(0, 33, 0, 0);
            CommandBar2.Visibility = Visibility.Collapsed;
            Shadow2.Visibility = Visibility.Collapsed;
            Shadow1.Visibility = Visibility.Collapsed;
            CloseFocusMode.Visibility = Visibility.Visible;

            Ad1.Suspend();
            Ad1.Visibility = Visibility.Collapsed;
            Ad.Visibility = Visibility.Collapsed;
        }
        private void CmdFocusMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchToFocusMode();
        }

        private void CloseFocusMode_Click(object sender, RoutedEventArgs e)
        {
            Text1.Margin = new Thickness(0, 74, 0, 40);
            Text1.SetValue(Canvas.ZIndexProperty, 0);
            CommandBar2.Visibility = Visibility.Visible;
            Shadow2.Visibility = Visibility.Visible;
            Shadow1.Visibility = Visibility.Visible;
            CloseFocusMode.Visibility = Visibility.Collapsed;

            if (AdRemove != "Paid")
            {
                Ad1.Resume();
                Ad1.Visibility = Visibility.Visible;
                Ad.Visibility = Visibility.Visible;
                Text1.Margin = new Thickness(0, 74, 0, 130);
            }
        }

        private void LaunchOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
             localSettings.Values["LaunchMode"] = LaunchOptions.SelectedValue;
        }

        private void DefaultFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["DefaultFileType"] = DefaultFileType.SelectedValue;

            DefaultFileExt = Convert.ToString(DefaultFileType.SelectedValue); //update the default file type right away
        }
    }
}
