using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using QuickPad.Mvvm.Models;
using Windows.Globalization;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Models.Theme;

namespace QuickPad.Mvvm.ViewModels
{
    public partial class SettingsViewModel : ViewModel
    {
        readonly ApplicationDataContainer _roamingSettings;
        private string _previousMode;
        private string _statusText;
        private ResourceLoader _resourceLoader;
        private Timer _statusCooldown;

        private IApplication App { get; }
        private IServiceProvider _serviceProvider;

        public SettingsViewModel(ILogger<SettingsViewModel> logger, IApplication app, IServiceProvider serviceProvider) : base(logger)
        {
            App = app;
            _serviceProvider = serviceProvider;
            AllFonts = new ObservableCollection<string>(
                CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font));

            AllDisplayModes = new ObservableCollection<DisplayMode>();

            Enum.GetNames(typeof(DisplayModes)).ToList().ForEach(uid => AllDisplayModes.Add(new DisplayMode(uid)));

            AllDisplayModes.Remove(AllDisplayModes.First(dm => dm.Uid == DisplayModes.LaunchNinjaMode.ToString()));

            _roamingSettings = ApplicationData.Current.RoamingSettings;

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguageModel>();
            foreach (var lang in supportedLang)
            {
                DefaultLanguages.Add(new DefaultLanguageModel(lang));
            }

            _statusCooldown = new Timer(StatusTimerCallback);
        }

        public string VersionNumberText => String.Format("{0}.{1}.{2}.{3}",Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

        private void StatusTimerCallback(object state)
        {
            Set(ref _statusText, App.CurrentViewModel.FilePath, nameof(StatusText));
        }

        protected bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = null)
        {
            var originalValue = (TValue)Get(default(TValue), propertyName);
            var currentValue = originalValue;

            if (!base.Set(ref currentValue, value, propertyName)) return false;

            if (propertyName != null && (!originalValue?.Equals(currentValue) ?? true))
            {
                _roamingSettings.Values[propertyName] = value;
            }

            return true;
        }

        protected virtual TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName != null && _roamingSettings.Values.ContainsKey(propertyName))
            {
                return (TValue)_roamingSettings.Values[propertyName];
            }

            return defaultValue;
        }

        [JsonIgnore]
        public ObservableCollection<DisplayMode> AllDisplayModes { get; }

        [JsonIgnore]
        public ObservableCollection<string> AllFonts { get; }

        [JsonIgnore]
        public IEnumerable<double> AllFontSizes { get; } =
            new double[] { 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        [JsonIgnore]
        public Action<double> AfterTintOpacityChanged { get; set; }

        [JsonIgnore]
        public ObservableCollection<DefaultLanguageModel> DefaultLanguages { get; }

        [JsonIgnore]
        public string StatusText { 
            get => _statusText;
            private set
            {
                if(Set(ref _statusText, value))
                {
                    _statusCooldown.Change(_countdown, TimeSpan.Zero);
                }
            }
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

            if (verbosity != Verbosity.Debug)
            {
                StatusText = message;
                _countdown = countdown;

                StatusTextColor = verbosity == Verbosity.Error ? new SolidColorBrush(Colors.Red) : DefaultStatusColor;
            }
        }

        [JsonIgnore]
        public SolidColorBrush StatusTextColor
        {
            get => _statusTextColor ??= DefaultStatusColor;
            set => _statusTextColor = value;
        }

        public SolidColorBrush DefaultStatusColor => Application.Current.Resources["SystemControlForegroundBaseMediumHighBrush"] as SolidColorBrush;

        public enum Verbosity { Debug, Release, Error }

        [NotifyOnReset]
        public string CustomThemeId
        {
            get => Get((string)null);
            set => Set(value);
        }

        public Color DefaultTextForegroundColor {
            get
            {
                var current = _serviceProvider.GetService<DefaultTextForegroundColor>().Color;

                var hex = Get(current.ToHex());
                hex = hex.Replace("#", string.Empty);
                var a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
                var r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
                var g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
                var b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

                return Color.FromArgb(a, r, g, b);
            }
            set
            {
                var current = _serviceProvider.GetService<DefaultTextForegroundColor>().Color;

                if (!Set(ref current, value)) return;

                _serviceProvider.GetService<DefaultTextForegroundColor>().Color = current;
                Set(value.ToHex());
            }
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
                {
                    return DefaultLanguages.FirstOrDefault(dl => dl.ID == language) ?? DefaultLanguages.FirstOrDefault();
                }

                return DefaultLanguages.FirstOrDefault();
            }
            set
            {
                if (Set(value.ID))
                {
                    ApplicationLanguages.PrimaryLanguageOverride = value.ID;
                }
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

        public FlowDirection FlowDirection
        {
            get => FlowDirection.TryParse(Get(nameof(FlowDirection.LeftToRight)), out FlowDirection result)
                ? result
                : FlowDirection.LeftToRight;
            set => Set(value.ToString());
        }

        private bool _showSettings;
        [JsonIgnore]
        public bool ShowSettings
        {
            get => _showSettings;
            set
            {
                if (Set(ref _showSettings, value))
                {
                    ShowSettingsTab = SettingsTabs.General;
                }
            }
        }

        [JsonIgnore]
        public SettingsTabs ShowSettingsTab 
        {
            get => _showSettingsTab;
            set => Set(ref _showSettingsTab, value);
        }

        public enum SettingsTabs
        {
            General, Theme, Fonts, Advanced, About
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
            set => Set(value);
        }

        public string DefaultModeText =>
            AllDisplayModes.FirstOrDefault(dm => dm.Uid == DefaultMode)?.Text ??
                AllDisplayModes.First().Text;

        [NotifyOnReset]
        public bool AcknowledgeFontSelectionChange
        {
            get => Get(false);
            set => Set(value);
        }

        private string _currentMode;
        private string _currentModeText;
        private TimeSpan _countdown;
        private SolidColorBrush _statusTextColor;
        private SettingsTabs _showSettingsTab = SettingsTabs.General;

        [JsonIgnore]
        public string CurrentMode
        {
            get => _currentMode ??= DefaultMode;
            set
            {
                var previousMode = _currentMode;
                if (!Set(ref _currentMode, value)) return;

                if (value == nameof(DisplayModes.LaunchCompactOverlay))
                {
                    ReturnToMode = previousMode;
                }
                
                if (_resourceLoader == null) _resourceLoader = ResourceLoader.GetForCurrentView();
                
                _currentModeText = _resourceLoader.GetString(_currentMode);

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
        public string ReturnToMode { get; set; } = nameof(DisplayModes.LaunchClassicMode);

        [JsonIgnore]
        public string CurrentModeText => _currentModeText;

        public bool StatusBar
        {
            get => Get(true);
            set
            {
                if (Set(value))
                {
                    OnPropertyChanged(nameof(ShowStatusBar));
                }
            }
        }

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowNinjaMode => CurrentMode.Equals(DisplayModes.LaunchNinjaMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowMenu => CurrentMode.Equals(DisplayModes.LaunchClassicMode.ToString(), StringComparison.InvariantCultureIgnoreCase) || ShowNinjaMode;

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowCommandBar => CurrentMode.Equals(DisplayModes.LaunchDefaultMode.ToString(), StringComparison.InvariantCultureIgnoreCase) || ShowNinjaMode;

        [JsonIgnore]
        [NotifyOnReset]
        public bool FocusMode => CurrentMode.Equals(DisplayModes.LaunchFocusMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool CompactOverlay => CurrentMode.Equals(DisplayModes.LaunchCompactOverlay.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowStatusBar => (ShowMenu && StatusBar) || (ShowCommandBar && StatusBar);

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

        private void NotifyAll()
        {
            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CustomAttributes.Any(ca => ca.AttributeType == typeof(NotifyOnResetAttribute)))
                .ToList().ForEach(
                    info =>
                    {
                        OnPropertyChanged(info.Name);
                    });
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

                JsonConvert.DeserializeAnonymousType<SettingsViewModel>(json, this);

                NotifyAll();
            }
        }

        public async Task ExportSettings()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("JSON File", new List<string>() { ".json" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "Quick-Pad-Settings";

            var file = await savePicker.PickSaveFileAsync();

            using var stream = await file.OpenStreamForWriteAsync();

            stream.Write(bytes, 0, bytes.Length);
            await stream.FlushAsync();
            stream.Close();
        }
    }
}
