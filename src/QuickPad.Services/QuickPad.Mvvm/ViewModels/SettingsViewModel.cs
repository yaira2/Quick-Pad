using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.Models.Theme;

namespace QuickPad.Mvvm.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        private readonly ApplicationDataContainer _roamingSettings;
        private readonly Timer _statusCooldown;
        private TimeSpan _countdown;
        private string _currentMode;
        private string _defaultColor;
        private ResourceLoader _resourceLoader;

        private bool _showSettings;
        private SettingsTabs _showSettingsTab = SettingsTabs.General;
        private string _statusText;
        private SolidColorBrush _statusTextColor;

        public bool ShowCompactOverlayTip = SystemInformation.IsFirstRun;
        private string _returnToMode = null;

        public SettingsViewModel(ILogger<SettingsViewModel> logger, IApplication app, IServiceProvider serviceProvider)
            : base(logger)
        {
            App = app;
            ServiceProvider = serviceProvider;
            AllFonts = new ObservableCollection<string>(
                CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font));

            AllDisplayModes = new ObservableCollection<DisplayMode>();

            Enum.GetNames(typeof(DisplayModes)).ToList().ForEach(uid => AllDisplayModes.Add(new DisplayMode(uid)));

            AllDisplayModes.Remove(AllDisplayModes.First(dm => dm.Uid == DisplayModes.LaunchNinjaMode.ToString()));

            _roamingSettings = ApplicationData.Current.RoamingSettings;

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguageModel>();
            foreach (var lang in supportedLang) DefaultLanguages.Add(new DefaultLanguageModel(lang));

            _statusCooldown = new Timer(StatusTimerCallback);
        }

        [JsonIgnore]
        private IApplication App { get; }

        [JsonIgnore]
        private IServiceProvider ServiceProvider { get; }

        [JsonIgnore]
        public string VersionNumberText =>
            $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

        [JsonIgnore] 
        public ObservableCollection<DisplayMode> AllDisplayModes { get; }

        [JsonIgnore] 
        public ObservableCollection<string> AllFonts { get; }

        [JsonIgnore]
        public IEnumerable<double> AllFontSizes { get; } =
            new double[] {4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};

        [JsonIgnore] 
        public Action<double> AfterTintOpacityChanged { get; set; }

        [JsonIgnore] 
        public ObservableCollection<DefaultLanguageModel> DefaultLanguages { get; }

        [JsonIgnore]
        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (Set(ref _statusText, value)) _statusCooldown.Change(_countdown, TimeSpan.Zero);
            }
        }

        [JsonIgnore]
        public SolidColorBrush StatusTextColor
        {
            get => _statusTextColor ??= DefaultStatusColor;
            set => _statusTextColor = value;
        }

        [JsonIgnore]
        public SolidColorBrush DefaultStatusColor =>
            Application.Current.Resources["SystemControlForegroundBaseMediumHighBrush"] as SolidColorBrush;


        [NotifyOnReset]
        public string CustomThemeId
        {
            get => Get((string) null);
            set => Set(value);
        }

        [NotifyOnReset]
        [JsonIgnore]
        public Color DefaultTextForegroundColor
        {
            get
            {
                var hex = DefaultTextForegroundColorString;

                hex = hex.Replace("#", string.Empty);
                var a = (byte)Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte)Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte)Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte)Convert.ToUInt32(hex.Substring(6, 2), 16);

                return Color.FromArgb(a, r, g, b);
            }
            set
            {
                var obj = ServiceProvider.GetService<DefaultTextForegroundColor>();
                var current = obj.Color;

                if (!Set(ref current, value, nameof(DefaultTextForegroundColorString))) return;

                obj.Color = current;
                if (Set(value.ToHex(), nameof(DefaultTextForegroundColorString)))
                {
                    NotifyThemeChanged();
                }
            }
        }

        public string DefaultTextForegroundColorString 
        { 
            get => Get(Colors.White.ToHex());
            set => Set(value);
        }

        [NotifyOnReset]
        [JsonIgnore]
        public SolidColorBrush DefaultTextForegroundBrush
        {
            get
            {
                _defaultColor ??= ServiceProvider.GetService<IVisualThemeSelector>().CurrentItem
                    .DefaultTextForegroundColor.ToHex();

                var hex = DefaultTextForegroundBrushString;

                hex = hex.Replace("#", string.Empty);
                var a = (byte) Convert.ToUInt32(hex.Substring(0, 2), 16);
                var r = (byte) Convert.ToUInt32(hex.Substring(2, 2), 16);
                var g = (byte) Convert.ToUInt32(hex.Substring(4, 2), 16);
                var b = (byte) Convert.ToUInt32(hex.Substring(6, 2), 16);

                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            set => Set(value.Color.ToHex(), nameof(DefaultTextForegroundBrushString));
        }

        public string DefaultTextForegroundBrushString
        {
            get => Get(_defaultColor);
            set => Set(value);
        }

        [NotifyOnReset]
        public double BackgroundTintOpacity
        {
            get => Get(0.75);
            set
            {
                Set(value);
                AfterTintOpacityChanged?.Invoke(value);
            }
        }

        [NotifyOnReset]
        public bool PasteTextOnly
        {
            get => Get(true);
            set => Set(value);
        }

        [NotifyOnReset]
        public DefaultLanguageModel DefaultLanguage
        {
            get
            {
                var language = Get((string) null);
                if (language != null)
                    return DefaultLanguages.FirstOrDefault(dl => dl.ID == language) ??
                           DefaultLanguages.FirstOrDefault();

                return DefaultLanguages.FirstOrDefault();
            }
            set
            {
                if (Set(value.ID)) ApplicationLanguages.PrimaryLanguageOverride = value.ID;
            }
        }

        [NotifyOnReset]
        public string DefaultFont
        {
            get => Get("Consolas");
            set => Set(value);
        }

        [NotifyOnReset]
        public string DefaultRtfFont
        {
            get => Get("Calibri");
            set => Set(value);
        }

        [NotifyOnReset]
        public bool SpellCheck
        {
            get => Get(false);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool WordWrap
        {
            get => Get(false);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool RtfSpellCheck
        {
            get => Get(true);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool RtfWordWrap
        {
            get => Get(true);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool UseAcrylic
        {
            get => Get(false);
            set => Set(value);
        }

        [NotifyOnReset]
        public double DefaultFontRtfSize
        {
            get => Get(12.0);
            set => Set(value);
        }

        [NotifyOnReset]
        public double DefaultFontSize
        {
            get => Get(14.0);
            set => Set(value);
        }

        [NotifyOnReset]
        public FlowDirection FlowDirection
        {
            get => Enum.TryParse(Get(nameof(FlowDirection.LeftToRight)), out FlowDirection result)
                ? result
                : FlowDirection.LeftToRight;
            set => Set(value.ToString());
        }

        [JsonIgnore]
        public bool ShowSettings
        {
            get => _showSettings;
            set
            {
                if (Set(ref _showSettings, value)) ShowSettingsTab = SettingsTabs.General;
            }
        }

        [JsonIgnore]
        public SettingsTabs ShowSettingsTab
        {
            get => _showSettingsTab;
            set => Set(ref _showSettingsTab, value);
        }


        [NotifyOnReset]
        public bool AutoSave
        {
            get => Get(true);
            set => Set(value);
        }

        [NotifyOnReset]
        public int AutoSaveFrequency
        {
            get => Get(10);
            set => Set(value);
        }

        [NotifyOnReset]
        public string DefaultFileType
        {
            get => Get(".txt");
            set => Set(value);
        }

        [NotifyOnReset]
        public string DefaultEncoding
        {
            get => Get("Utf8");
            set => Set(value);
        }

        [NotifyOnReset]
        public string DefaultMode
        {
            get => Get(DisplayModes.LaunchClassicMode.ToString());
            set
            {
                if (Set(value))
                {
                    CurrentMode = value;
                }
            }
        }

        [JsonIgnore]
        public string DefaultModeText =>
            AllDisplayModes.FirstOrDefault(dm => dm.Uid == DefaultMode)?.Text ??
            AllDisplayModes.First().Text;

        [NotifyOnReset]
        public bool AcknowledgeFontSelectionChange
        {
            get => Get(false);
            set => Set(value);
        }

        [JsonIgnore]
        public string CurrentMode
        {
            get => _currentMode ??= DefaultMode;
            set
            {
                var previousMode = _currentMode;
                if (!Set(ref _currentMode, value)) return;

                if (value == nameof(DisplayModes.LaunchCompactOverlay)) ReturnToMode = previousMode;

                if (_resourceLoader == null) _resourceLoader = ResourceLoader.GetForCurrentView();

                CurrentModeText = _resourceLoader.GetString(_currentMode);

                OnPropertyChanged(nameof(CurrentMode));
                OnPropertyChanged(nameof(CurrentModeText));
                OnPropertyChanged(nameof(ShowMenu));
                OnPropertyChanged(nameof(ShowCommandBar));
                OnPropertyChanged(nameof(FocusMode));
                OnPropertyChanged(nameof(CompactOverlay));
                OnPropertyChanged(nameof(ShowStatusBar));
                OnPropertyChanged(nameof(TitleMargin));
            }
        }

        [JsonIgnore]
        public string ReturnToMode
        {
            get => _returnToMode ??= DefaultMode;
            set => _returnToMode = value;
        }

        [JsonIgnore] 
        public string CurrentModeText { get; private set; }

        [JsonIgnore]
        public bool StatusBar
        {
            get => Get(true);
            set
            {
                if (Set(value)) OnPropertyChanged(nameof(ShowStatusBar));
            }
        }

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowNinjaMode => CurrentMode.Equals(DisplayModes.LaunchNinjaMode.ToString(),
            StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowMenu =>
            CurrentMode.Equals(DisplayModes.LaunchClassicMode.ToString(),
                StringComparison.InvariantCultureIgnoreCase) || ShowNinjaMode;

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowCommandBar =>
            CurrentMode.Equals(DisplayModes.LaunchDefaultMode.ToString(),
                StringComparison.InvariantCultureIgnoreCase) || ShowNinjaMode;

        [JsonIgnore]
        [NotifyOnReset]
        public bool FocusMode => CurrentMode.Equals(DisplayModes.LaunchFocusMode.ToString(),
            StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool CompactOverlay => CurrentMode.Equals(DisplayModes.LaunchCompactOverlay.ToString(),
            StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore] 
        [NotifyOnReset] 
        public bool ShowStatusBar => ShowMenu && StatusBar || ShowCommandBar && StatusBar;

        [JsonIgnore]
        [NotifyOnReset]
        public Thickness TitleMargin => FocusMode ? new Thickness(BackButtonWidth, 0, 0, 0) : new Thickness(0);

        [JsonIgnore] 
        public double BackButtonWidth { get; set; }

        [JsonIgnore]
        public bool NotDeferred
        {
            get => Get(true);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool EnableGoogleSearch
        {
            get => Get(false);
            set => Set(value);
        }

        private void StatusTimerCallback(object state)
        {
            Set(ref _statusText, App.CurrentViewModel.FilePath, nameof(StatusText));
        }

        protected bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = null)
        {
            var originalValue = Get(default(TValue), propertyName);
            var currentValue = originalValue;

            if (!base.Set(ref currentValue, value, propertyName)) return false;

            if (propertyName != null && (!originalValue?.Equals(currentValue) ?? true))
                _roamingSettings.Values[propertyName] = value;

            return true;
        }

        protected virtual TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName != null && _roamingSettings.Values.ContainsKey(propertyName))
                return (TValue) _roamingSettings.Values[propertyName];

            return defaultValue;
        }

        public void Status(string message, TimeSpan countdown, Verbosity verbosity)
        {
            if (verbosity == Verbosity.Debug)
            {
#if DEBUG
                StatusText = message;
                _countdown = countdown;
#endif
            }

            if (verbosity == Verbosity.Debug) return;

            StatusText = message;
            _countdown = countdown;

            StatusTextColor = verbosity == Verbosity.Error
                ? new SolidColorBrush(Colors.Red)
                : DefaultStatusColor;
        }

        private void NotifyAll()
        {
            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CustomAttributes.Any(ca => ca.AttributeType == typeof(NotifyOnResetAttribute)))
                .ToList().ForEach(
                    info => { OnPropertyChanged(info.Name); });
        }

        public void ResetSettings()
        {
            _roamingSettings.Values.Clear();

            NotifyAll();
        }

        public async Task ImportSettings()
        {
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

                JsonConvert.DeserializeAnonymousType(json, this);

                NotifyAll();
            }
        }

        public async Task ExportSettings()
        {
            var jsonConfiguration = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };

            var json = JsonConvert.SerializeObject(this, Formatting.Indented, jsonConfiguration);
            var bytes = Encoding.UTF8.GetBytes(json);

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("JSON File", new List<string> {".json"});
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Quick-Pad-Settings";

            var file = await savePicker.PickSaveFileAsync();

            using var stream = await file.OpenStreamForWriteAsync();

            stream.Write(bytes, 0, bytes.Length);
            await stream.FlushAsync();
            stream.Close();
        }

        public void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(DefaultTextForegroundColor));
            OnPropertyChanged(nameof(DefaultTextForegroundBrush));
        }
    }
}