using Microsoft.AppCenter.Analytics;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WINUI = Microsoft.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.Dialog
{
    public sealed partial class Settings : UserControl, INotifyPropertyChanged
    {
        #region Property notification
        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public ResourceLoader textResource { get; } = ResourceLoader.GetForCurrentView(); //Use to get a text resource from Strings/en-US

        public QuickPad.VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

        Setting _setting = null;
        public QuickPad.Setting QSetting
        {
            get
            {
                if (_setting is null)
                {
                    _setting = ((Window.Current.Content as Frame).Content as MainPage).QSetting;
                }
                return _setting;
            }
            set
            {
                Set(ref _setting, value);
            }
        }

        public MainPage mainPage => ((Window.Current.Content as Frame).Content as MainPage);

        public Settings()
        {
            this.InitializeComponent();

            AllFonts = new ObservableCollection<FontFamilyItem>(CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font).Select(font => new FontFamilyItem(font)));

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguage>();
            foreach (var lang in supportedLang)
            {
                DefaultLanguages.Add(new DefaultLanguage(lang));
            }

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
        }

        #region Page navigation
        settingPage _sp = settingPage.General;
        public settingPage CurrentSettingPage
        {
            get => _sp;
            set => Set(ref _sp, value);
        }

        WINUI.NavigationViewItem _select_nav;
        public WINUI.NavigationViewItem SelectedNavigationItem
        {
            get => _select_nav;
            set
            {
                if (!Equals(value, _select_nav))
                {
                    Set(ref _select_nav, value);
                    if (value is null)
                        return;
                    CurrentSettingPage = (settingPage)Enum.Parse(typeof(settingPage), value.Tag.ToString());
                    
                }
            }
        }

        public bool SyncWithPage(settingPage current, string name) => current.ToString() == name;

        public Visibility ShowIfItWasThePage(settingPage current, string name) => current.ToString() == name ? Visibility.Visible : Visibility.Collapsed;
        #endregion

        #region Settings management
        public async void ResetSettings()
        {
            //Ask if they want to save change first
            if (mainPage.Changed)
            {
                await mainPage.WantToSave.ShowAsync();
                switch (mainPage.WantToSave.DialogResult)
                {
                    case DialogResult.Yes:
                        await mainPage.SaveWork();
                        break;
                    case DialogResult.Cancel:
                        return;
                }
            }
            mainPage.timer.Stop();//Prevent some change made to setting
            QSetting.ResetSettings();
        }

        public async void ExportSettings()
        {
            string result = QSetting.ExportSetting();
            Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker()
            {
                SuggestedFileName = $"AppConfig_{DateTime.Now.ToString("ddMMyy_HHmmss")}"
            };
            picker.FileTypeChoices.Add("App configurations", new List<string>() { ".txt" });
            try
            {
                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                //Show error
            }
        }

        public async void ImportSetting()
        {
            Windows.Storage.Pickers.FileOpenPicker open = new Windows.Storage.Pickers.FileOpenPicker()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            open.FileTypeFilter.Add(".txt");
            var file = await open.PickSingleFileAsync();
            if (file != null)
            {
                mainPage.timer.Stop();
                var setting = await FileIO.ReadTextAsync(file);
                QSetting.ImportSetting(setting);
            }
            Application.Current.Exit();
        }
        #endregion

        #region Languages
        int? _def_lang = null;
        public int SelectedDefaultLanguage
        {
            get
            {
                if (_def_lang is null)
                {
                    _def_lang = DefaultLanguages.IndexOf(DefaultLanguages.First(i => i.ID == QSetting.AppLanguage));
                }
                return _def_lang.Value;
            }
            set
            {
                if (!Equals(_def_lang, value))
                {
                    Set(ref _def_lang, value);
                    if (value == -1)
                        return;
                    ApplicationLanguages.PrimaryLanguageOverride = DefaultLanguages[value].ID;
                    LangChangeNeedRestart.Visibility =
                        ApplicationLanguages.PrimaryLanguageOverride == DefaultLanguages[value].ID
                        ? Visibility.Collapsed : Visibility.Visible;
                    QSetting.AppLanguage = DefaultLanguages[value].ID;
                }
            }
        }
        
        private ObservableCollection<DefaultLanguage> _DefaultLanguage;
        public ObservableCollection<DefaultLanguage> DefaultLanguages
        {
            get => _DefaultLanguage;
            set => Set(ref _DefaultLanguage, value);
        }
        #endregion

        #region Fonts
        ObservableCollection<FontFamilyItem> _fonts;
        public ObservableCollection<FontFamilyItem> AllFonts
        {
            get => _fonts;
            set => Set(ref _fonts, value);
        }

        public FontFamilyItem FromStringToFontItem(string input)
        {
            FontFamilyItem item = mainPage.AllFonts.First(i => i.Name == input);
            if (item is null)
                return new FontFamilyItem("Consolas");
            return item;
        }

        public void FromFontItemBackToString(object font)
        {
            FontFamilyItem selected = (font as FontFamilyItem);
            QSetting.DefaultFont = selected.Name;
            //
            //set default font to UIs that still not depend on binding
            mainPage.Fonts.PlaceholderText = selected.Name;
            mainPage.Fonts.SelectedItem = selected.Name;
            mainPage.FontSelected.Text = selected.Name;
            mainPage.Text1.Document.Selection.CharacterFormat.Name = selected.Name;
        }
        #endregion

        #region Font color

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
                    if (value < 0)
                        return;
                    //Update setting
                    if (FontColorCollections.Count < 1)
                    {
                        //Delay reselect
                        ReUpdateSelectedDefaultFontColor(value);
                        return;
                    }
                    QSetting.DefaultFontColor = FontColorCollections[value].TechnicalName;
                }
            }
        }

        private async void ReUpdateSelectedDefaultFontColor(int selection)
        {
            while (FontColorCollections.Count < 1)
            {
                await Task.Delay(200);
            }
            SelectedDefaultFontColor = selection;
        }
        #endregion


        public string VersionNumberText => string.Format(textResource.GetString("VersionFormat"), Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

        private async void CmdReview_Click(object sender, RoutedEventArgs e)
        {
            await ShowRatingReviewDialog(); //show the review dialog.

            Analytics.TrackEvent("User clicked on review"); //log even in app center
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
    }

    public enum settingPage
    {
        General,
        Theme,
        Font,
        Advanced,
        Report,
        About
    }
}
