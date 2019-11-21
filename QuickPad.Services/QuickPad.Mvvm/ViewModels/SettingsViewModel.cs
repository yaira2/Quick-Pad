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

namespace QuickPad.Mvvm.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        readonly ApplicationDataContainer _roamingSettings;

        [JsonIgnore]
        public ObservableCollection<DisplayMode> AllDisplayModes { get; }

        public SettingsViewModel(ILogger<SettingsViewModel> logger) : base(logger)
        {
            AllFonts = new ObservableCollection<string>(
                CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font));

            AllDisplayModes = new ObservableCollection<DisplayMode>();

            Enum.GetNames(typeof(DisplayModes)).ToList().ForEach(uid => AllDisplayModes.Add(new DisplayMode(uid)));

            AllDisplayModes.Remove(AllDisplayModes.First(dm => dm.Uid == DisplayModes.LaunchFullUIMode.ToString()));

            _roamingSettings = ApplicationData.Current.RoamingSettings;

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguageModel>();
            foreach (var lang in supportedLang)
            {
                DefaultLanguages.Add(new DefaultLanguageModel(lang));
            }

            _statusCooldown = new Timer(StatusTimerCallback);
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

        private SettingsViewModel() : base(null) { }

        [JsonIgnore]
        public ObservableCollection<string> AllFonts { get; }

        [JsonIgnore]
        public IEnumerable<double> AllFontSizes { get; } =
            new double[] { 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        [JsonIgnore]
        public Action<double> AfterTintOpacityChanged { get; set; }

        [JsonIgnore]
        public ObservableCollection<DefaultLanguageModel> DefaultLanguages { get; }

        private Timer _statusCooldown;

        private void StatusTimerCallback(object state) 
        {
            Set(ref _status, string.Empty, nameof(Status));
        }

        [JsonIgnore]
        public string Status { 
            get => _status;
            set
            {
                if(Set(ref _status, value))
                {
                    _statusCooldown.Change(TimeSpan.FromSeconds(30), TimeSpan.Zero);
                }
            }
        }

        [NotifyOnReset]
        public string CustomThemeId
        {
            get => Get((string)null);
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
            internal set => Set(value);
        }

        [NotifyOnReset]
        public DefaultLanguageModel DefaultLanguage
        {
            get => Get((DefaultLanguageModel)null);
            set => Set(value);
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
        public bool ShowSettings
        {
            get => Get(false);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool ModeByFileType
        {
            get => Get(false);
            set => Set(value);
        }

        [NotifyOnReset]
        public bool AutoSave
        {
            get => Get(false);
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
        [JsonIgnore]
        public string CurrentMode
        {
            get => _currentMode ??= DefaultMode;
            set
            {
                var previousMode = _currentMode;
                if (!Set(ref _currentMode, value)) return;
                
                if (_resourceLoader == null) _resourceLoader = ResourceLoader.GetForCurrentView();
                
                _currentModeText = _resourceLoader.GetString(_currentMode);

                _previousMode = previousMode;

                OnPropertyChanged(nameof(CurrentMode));
                OnPropertyChanged(nameof(CurrentModeText));
                OnPropertyChanged(nameof(ShowMenu));
                OnPropertyChanged(nameof(ShowCommandBar));
                OnPropertyChanged(nameof(FocusMode));
                OnPropertyChanged(nameof(ShowStatusBar));
                OnPropertyChanged(nameof(TitleMargin));
            }
        }

        [JsonIgnore]
        public string CurrentModeText => _currentModeText;

        public bool StatusBar
        {
            get => Get(true);
            set => Set(value);
        }

        private string _previousMode;
        private string _status;
        private string _currentModeText;
        private ResourceLoader _resourceLoader;

        [JsonIgnore]
        public string PreviousMode => _previousMode ??= DisplayModes.LaunchClassicMode.ToString();

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowFullUI => CurrentMode.Equals(DisplayModes.LaunchFullUIMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowMenu => CurrentMode.Equals(DisplayModes.LaunchClassicMode.ToString(), StringComparison.InvariantCultureIgnoreCase) || ShowFullUI;

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowCommandBar => CurrentMode.Equals(DisplayModes.LaunchDefaultMode.ToString(), StringComparison.InvariantCultureIgnoreCase) || ShowFullUI;

        [JsonIgnore]
        [NotifyOnReset]
        public bool FocusMode => CurrentMode.Equals(DisplayModes.LaunchFocusMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool CompactOverlay => CurrentMode.Equals(DisplayModes.LaunchOnTopMode.ToString(), StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        [NotifyOnReset]
        public bool ShowStatusBar => (ShowMenu && StatusBar) || (ShowCommandBar && StatusBar);

        [JsonIgnore]
        [NotifyOnReset]
        public Thickness TitleMargin => FocusMode ? new Thickness(BackButtonWidth, 0, 0, 0) : new Thickness(0);

        public double BackButtonWidth { get; set; }

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
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
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

            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
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
