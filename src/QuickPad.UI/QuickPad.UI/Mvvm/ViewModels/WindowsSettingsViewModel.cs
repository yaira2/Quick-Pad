using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using QuickPad.Mvc;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Managers;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Theme;

namespace QuickPad.UI.Helpers
{
    public class WindowsSettingsViewModel : SettingsViewModel<StorageFile, IRandomAccessStream>
    {
        private ResourceLoader _resourceLoader;
        private SolidColorBrush _statusTextColor;

        protected new WindowsSettingsModel Model
        {
            get => base.Model as WindowsSettingsModel;
            set => base.Model = value;
        }

        public WindowsSettingsViewModel(
            ILogger<SettingsViewModel<StorageFile, IRandomAccessStream>> logger
            , IServiceProvider serviceProvider
            , IApplication<StorageFile, IRandomAccessStream> app)
            : base(logger, serviceProvider, app)
        {
            ServiceProvider = serviceProvider;
            
            Model = new WindowsSettingsModel(logger, app, serviceProvider);

            AllFonts = new ObservableCollection<string>(
                CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font));

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguageModel>();
            foreach (var lang in supportedLang) DefaultLanguages.Add(new DefaultLanguageModel(lang));

            AllDisplayModes = new ObservableCollection<DisplayMode>();

            Enum.GetNames(typeof(DisplayModes)).ToList().ForEach(uid => AllDisplayModes.Add(new DisplayMode(uid)));
        }

        [NotifyOnReset]
        [JsonIgnore]
        public DefaultLanguageModel DefaultLanguage
        {
            get
            {
                var language = Model.DefaultLanguageString;
                if (language != null)
                    return DefaultLanguages.FirstOrDefault(dl => dl.ID == language) ??
                           DefaultLanguages.FirstOrDefault();

                return DefaultLanguages.FirstOrDefault();
            }
            set
            {
                if (value.ID != Model.DefaultLanguageString)
                {
                    Model.DefaultLanguageString = value.ID;
                    ApplicationLanguages.PrimaryLanguageOverride = value.ID;
                }
            }
        }

        [NotifyOnReset]
        [JsonIgnore]
        public FlowDirection FlowDirection
        {
            get => Enum.TryParse(Model.FlowDirection, out FlowDirection result)
                ? result
                : FlowDirection.LeftToRight;
            set => Model.FlowDirection = value.ToString();
        }
        
        [JsonIgnore]
        [NotifyOnReset]
        public Thickness TitleMargin => FocusMode
            ? new Thickness(BackButtonWidth, 0, 0, 0)
            : new Thickness(0);

        [JsonIgnore]
        public bool ShowCompactOverlayTip { get; set; } = SystemInformation.IsFirstRun;

        [JsonIgnore] 
        public ObservableCollection<DisplayMode> AllDisplayModes { get; }

        [JsonIgnore]
        public SolidColorBrush StatusTextColor
        {
            get => _statusTextColor ??= DefaultStatusColor;
            set => _statusTextColor = value;
        }

        [JsonIgnore]
        public SolidColorBrush DefaultStatusColor =>
            Application.Current.Resources["SystemControlForegroundBaseMediumHighBrush"] as SolidColorBrush;

        [JsonIgnore] 
        public IServiceProvider ServiceProvider { get; }

        [JsonIgnore]
        public string VersionNumberText =>
            $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        [NotifyOnReset]
        [JsonIgnore]
        public override string DefaultTextForegroundColor
        {
            get => Model.DefaultTextForegroundColorString;
            set
            {
                var obj = ServiceProvider.GetService<DefaultTextForegroundColor>();
                var current = obj.Color.ToHex();

                if (current == value) return;

                obj.Color = FromHex(value);
                Model.DefaultTextForegroundColorString = value;
                NotifyThemeChanged();
            }
        }

        [JsonIgnore]
        [NotifyOnReset]
        public string DefaultModeText =>
            AllDisplayModes.FirstOrDefault(dm => dm.Uid == DefaultMode)?.Text ??
            AllDisplayModes.First().Text;

        [NotifyOnReset]
        [JsonIgnore]
        public SolidColorBrush DefaultTextForegroundBrush
        {
            get
            {
                var hex = Model.DefaultTextForegroundBrushString;

                hex = hex.Replace("#", string.Empty);
                var a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            set => Model.DefaultTextForegroundBrushString = value.Color.ToHex();
        }

        // Not Ignored

        [NotifyOnReset]
        public double BackgroundTintOpacity
        {
            get => Model.BackgroundTintOpacity;
            set
            {
                Model.BackgroundTintOpacity = value;
                AfterTintOpacityChanged?.Invoke(value);
            }
        }

        // Helper Methods
        private Color FromHex(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            var a = (byte) Convert.ToUInt32(hex.Substring(0, 2), 16);
            var r = (byte) Convert.ToUInt32(hex.Substring(2, 2), 16);
            var g = (byte) Convert.ToUInt32(hex.Substring(4, 2), 16);
            var b = (byte) Convert.ToUInt32(hex.Substring(6, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }

        public override void ResetSettings()
        {
            Model.ResetSettings();

            App.SettingsViewModel.NotifyAll();
        }

        public override void LaunchUri(Uri uri)
        {
            Launcher.LaunchUriAsync(uri);
        }

        public override async Task ImportSettings()
        {
            if (ServiceProvider.GetService<DialogManager>().CurrentDialogView != null)
            {
                Logger.LogCritical("There is already a dialog open.");
                return;
            }

            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".json");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                using var jsonFile = await file.OpenReadAsync();

                var json = await jsonFile.ReadTextAsync();

                var quickPadSettingsResolver = new QuickPadSettingsResolver(ServiceProvider);

                JsonConvert.DeserializeAnonymousType(json, Model, new JsonSerializerSettings
                {
                    ContractResolver = quickPadSettingsResolver
                });

                NotifyAll();
            }
        }

        public override async Task ExportSettings()
        {
            if (ServiceProvider.GetService<DialogManager>().CurrentDialogView != null)
            {
                Logger.LogCritical("There is a dialog already open.  Cannot open another.");
                return;
            }

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("JSON File", new List<string> { ".json" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Quick-Pad-Settings";

            var file = await savePicker.PickSaveFileAsync();
            using var stream = await file.OpenStreamForWriteAsync();

            try
            {
                var jsonConfiguration = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                };

                var json = JsonConvert.SerializeObject(Model, Formatting.Indented, jsonConfiguration);
                var bytes = Encoding.UTF8.GetBytes(json);


                stream.Write(bytes, 0, bytes.Length);
                await stream.FlushAsync();
            }
            catch (Exception)
            {

            }
            finally
            {
                stream.Close();
            }
        }

        public void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(DefaultTextForegroundColor));
            OnPropertyChanged(nameof(DefaultTextForegroundBrush));
        }

        public override string GetTranslation(string resourceName)
        {
            if (_resourceLoader == null) _resourceLoader = ResourceLoader.GetForCurrentView();

            return _resourceLoader.GetString(resourceName);
        }
    }
}