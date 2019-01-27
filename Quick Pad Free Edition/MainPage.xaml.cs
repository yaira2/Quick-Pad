using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel.UserActivities;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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

        string UpdateFile;
        string AdRemove;
        private StoreContext context = null;
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(900, 900);

            //defualt filee name is "New Document"
            UpdateFile = "New Document";
            TQuick.Text = UpdateFile;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            //get some theme settings in
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            String localValue = localSettings.Values["Theme"] as string;

            if (localValue == "Light")
            {
                this.RequestedTheme = ElementTheme.Light;
                Light.IsChecked = true;
            }
            if (localValue == "Dark")
            {
                this.RequestedTheme = ElementTheme.Dark;
                Dark.IsChecked = true;
            }
            if (localValue == "System Defult")
            {
                this.RequestedTheme = ElementTheme.Default;
                SystemDefult.IsChecked = true;
            }

            CheckIfPaidForNoAds();

            //check if it is a new user
            String NewUser = localSettings.Values["NewUser"] as string;
            if (NewUser == "1")
            {
                localSettings.Values["NewUser"] = "2";
                NewUserFeedbackAsync();
            }
            else
            {
                if (NewUser == "0")
                {
                    localSettings.Values["NewUser"] = "1";
                }
            }
            if (NewUser != "0" && NewUser != "1" && NewUser != "2")
            {
                localSettings.Values["NewUser"] = "0";
            }

            //check if the setting is to launch in compact overlay mode
            String launchValue = localSettings.Values["LaunchMode"] as string;

            if (launchValue == "OnTop")
            {
                //launch compact overlay mode
                CompactOverlay.IsChecked = true;

                LaunchModeSwitch.IsOn = true;
            }
            else
            {
                LaunchModeSwitch.IsOn = false;
            }

           

            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested +=
        async (sender, args) =>
        {
            if (TQuick.Text == UpdateFile)
            {
                App.Current.Exit();
            };
            args.Handled = true;

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
                App.Current.Exit();
            }
            else
            {
                App.Current.Exit();
            }
        };


            //check for push notifications
            CheckPushNotifications();

        }

        public async void CheckPushNotifications()
        {
             //check for push notifications
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

            Windows.Storage.Streams.IRandomAccessStream randAccStream =
         await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            // Load the file into the Document property of the RichEditBox.
            Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);

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
            UpdateFile = "New Document";
            TQuick.Text = UpdateFile;
        }

        private async void CmdOpen_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    Windows.Storage.Streams.IRandomAccessStream randAccStream =
                await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                    UpdateFile = file.DisplayName;
                    TQuick.Text = UpdateFile;

                    // Load the file into the Document property of the RichEditBox.
                    Text1.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
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

        public async 
        Task
SaveWork()
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
                // Prevent updates to the remote version of the file until we 
                // finish making changes and call CompleteUpdatesAsync.
                // Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file

                ///////////////////////////////
                //Windows.Storage.Streams.IRandomAccessStream randAccStream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                // Text1.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
                //////////////////////////////
                Text1.Document.GetText(TextGetOptions.FormatRtf, out var value);

                UpdateFile = file.DisplayName;
                TQuick.Text = UpdateFile;

                await FileIO.WriteTextAsync(file, value);

                // Let Windows know that we're finished changing the file so the 
                // other app can update the remote version of the file.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    Windows.UI.Popups.MessageDialog errorBox =
                        new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                    await errorBox.ShowAsync();
                }
            }
        }

        public async void CmdSave_Click(object sender, RoutedEventArgs e)
        {
            await SaveWork();
        }

        private void CmdUndo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Undo();
        }

        private void CmdRedo_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Redo();
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
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
                if (text == "")
                {

                }
                else
                {
                    Text1.Document.Selection.TypeText(text);
                }
              
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text1.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(Text1.Document.Selection.Text);
            Text1.Document.Selection.Text = "";
            Clipboard.SetContent(dataPackage);
        }

        private async void CmdAbout_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await AboutDialog.ShowAsync();
        }

        private void SizeUp_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size + 2;
        }

        private void SizeDown_Click(object sender, RoutedEventArgs e)
        {
            if (Text1.Document.Selection.CharacterFormat.Size > 4)
            {
                Text1.Document.Selection.CharacterFormat.Size = Text1.Document.Selection.CharacterFormat.Size - 2;

            }
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
        }

        private void Emoji_Unchecked(object sender, RoutedEventArgs e)
        {
            Emoji2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void E1_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😀");

        }

        private void E2_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😁");
        }

        private void EmojiPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void E3_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😂");
        }

        private void E4_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤣");
        }

        private void E5_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😃");
        }

        private void E6_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😄");
        }

        private void E7_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😅");
        }

        private void E8_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😆");
        }

        private void E9_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😉");
        }

        private void E10_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😊");
        }

        private void E11_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😋");
        }

        private void E12_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😎");
        }

        private void E13_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😍");
        }

        private void E14_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😘");
        }

        private void E15_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🥰");
        }

        private void Fonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string myfont;
            myfont = ((ComboBoxItem)Fonts.SelectedItem).Content.ToString();
            Text1.Document.Selection.CharacterFormat.Name = myfont;
        }

        private void E16_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😗");
        }

        private void E17_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😙");
        }

        private void E18_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😚");
        }

        private void E19_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("☺");
        }

        private void E20_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🙂");
        }

        private void E21_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤗");
        }

        private void E22_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤩");
        }

        private void E23_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤔");
        }

        private void E24_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤨");
        }

        private void E25_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😐");
        }

        private void E26_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😑");
        }

        private void E27_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😶");
        }

        private void E28_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🙄");
        }

        private void E29_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😏");
        }

        private void E30_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😣");
        }

        private void E31_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😥");
        }

        private void E32_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😮");
        }

        private void E33_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤐");
        }

        private void E34_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😯");
        }

        private void E35_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😪");
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
            Emoji.IsChecked = false;
        }

        private void E36_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😫");
        }

        private void E37_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😴");
        }

        private void E38_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😌");
        }

        private void E39_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😛");
        }

        private void E40_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😜");
        }

        private void E41_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😝");
        }

        private void E42_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("🤤");
        }

        private void E43_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("😒");
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {

        }

        private void CmdShare_Click(object sender, RoutedEventArgs e)
        {
            Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
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
        }

        private async void RemoveAd_Click(object sender, RoutedEventArgs e)
        {
            // bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9PLQRS34KSGP"));

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
        }

        private void Light_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Light";
            this.RequestedTheme = ElementTheme.Light;
        }

        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "Dark";
            this.RequestedTheme = ElementTheme.Dark;
        }

        private void SystemDefult_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Theme"] = "System Defult";
            this.RequestedTheme = ElementTheme.Default;
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

        private void K1_Click(object sender, RoutedEventArgs e)
        {

            Text1.Document.Selection.TypeText(";)");
        }

        private void K2_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^_~");
        }

        private void K3_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";-)");
        }

        private void K4_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":)");
        }

        private void K5_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^_^");
        }

        private void K6_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-)");
        }

        private void K7_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":D");
        }

        private void K8_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^0^");
        }

        private void K9_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-D");
        }

        private void K11_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":P");
        }

        private void K12_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-P");
        }

        private void K13_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";P");
        }

        private void K14_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":(");
        }

        private void K15_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-(");
        }

        private void K16_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("U_U");
        }

        private void K17_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":[");
        }

        private void K18_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(">:(");
        }

        private void K19_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(">" + '\u0022' + "<");
        }

        private void K20_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":(");
        }
        private void K21_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-O");
        }

        private void K22_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("O.O");
        }

        private void K23_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-()");
        }

        private void K24_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("~_~");
        }

        private void K25_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^o^");
        }

        private void K26_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-S");
        }

        private void K27_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("<3");
        }

        private void K28_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^3^");
        }

        private void K29_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":-x");
        }

        private void K30_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":/");
        }

        private void K31_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("X_X");
        }

        private void K32_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("=/");
        }

        private void K33_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";(");
        }

        private void K34_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("T_T");
        }

        private void K35_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";[");
        }

        private void K36_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("+_+");
        }

        private void K37_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("O_O");
        }

        private void K38_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(":O");
        }

        private void K39_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("¬_¬");
        }

        private void K40_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";_;");
        }

        private void K41_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("=.=");
        }

        private void K42_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";]");
        }

        private void K43_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("^_+");
        }

        private void K44_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(";O)");
        }

        private void S1_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("(");
        }

        private void S2_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("[");
        }

        private void S3_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("{");
        }

        private void S4_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(")");
        }

        private void S5_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("]");
        }

        private void S6_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("}");
        }

        private void S7_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("<");
        }

        private void S8_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("‹");
        }

        private void S9_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("«");
        }

        private void S10_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText(">");
        }

        private void S11_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("›");
        }

        private void S12_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("»");
        }

        private void S13_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("⁅");
        }

        private void S14_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("⁽");
        }

        private void S15_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("©");
        }

        private void S16_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("⁆");
        }

        private void S17_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("⁾");
        }

        private void S18_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("®");
        }

        private void S19_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("|");
        }

        private void S20_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("¦");
        }

        private void S21_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("℗");
        }

        private void S22_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("+");
        }

        private void S23_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("-");
        }

        private void S24_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("™");
        }

        private void S25_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("×");
        }

        private void S26_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("÷");
        }

        private void S27_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("°");
        }

        private void S28_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("=");
        }

        private void S29_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("–");
        }

        private void S30_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("℃");
        }

        private void S31_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("'");
        }

        private void S32_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("—");
        }

        private void S33_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("℉");
        }

        private void S34_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("‘");
        }

        private void S35_Click(object sender, RoutedEventArgs e)
        {
            Text1.Document.Selection.TypeText("“");
        }

        private void Text1_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TQuick.Text = "*" + UpdateFile;
        }

        private async void Feedback_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }
    }
}
