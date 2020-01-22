using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuickPad.Mvvm.Models;

namespace QuickPad.Mvvm.ViewModels
{
    public abstract class SettingsViewModel<TStorageFile, TStream> : ViewModel<TStorageFile, TStream>
        where TStream : class
    {
        private readonly Timer _statusCooldown;
        private TimeSpan _countdown;
        private string _currentMode;

        private string _returnToMode;

        private bool _showSettings;
        private SettingsTabs _showSettingsTab = SettingsTabs.General;
        private string _statusText;
        private bool _statusBar = true;
        private bool _notDeferred = true;

        protected SettingsViewModel(
            ILogger<SettingsViewModel<TStorageFile, TStream>> logger
            , IServiceProvider serviceProvider
            , IApplication<TStorageFile, TStream> app)
            : base(logger, app)
        {
            ServiceProvider = serviceProvider;

            _statusCooldown = new Timer(StatusTimerCallback);
        }

        [JsonIgnore]
        public DocumentViewModel<TStorageFile, TStream> CurrentViewModel =>
            ServiceProvider.GetService<IApplication<TStorageFile, TStream>>().CurrentViewModel;

        [JsonIgnore]
        public string CurrentMode
        {
            get => _currentMode ??= DefaultMode;
            set
            {
                var previousMode = _currentMode;
                if (!Set(ref _currentMode, value)) return;

                if (value == nameof(DisplayModes.LaunchCompactOverlay)) ReturnToMode = previousMode;

                CurrentModeText = GetTranslation(_currentMode);

                OnPropertyChanged(nameof(CurrentMode));
                OnPropertyChanged(nameof(CurrentModeText));
                OnPropertyChanged(nameof(ShowMenu));
                OnPropertyChanged(nameof(ShowCommandBar));
                OnPropertyChanged(nameof(FocusMode));
                OnPropertyChanged(nameof(CompactOverlay));
                OnPropertyChanged(nameof(ShowStatusBar));
            }
        }

        [JsonIgnore]
        public string ReturnToMode
        {
            get => _returnToMode ??= DefaultMode;
            set => _returnToMode = value;
        }

        [JsonIgnore] [NotifyOnReset] public string CurrentModeText { get; private set; }

        [JsonIgnore]
        [NotifyOnReset]
        public bool StatusBar
        {
            get => _statusBar;
            set
            {
                if (Set(ref _statusBar, value, MethodBase.GetCurrentMethod().Name)) OnPropertyChanged(nameof(ShowStatusBar));
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

        [JsonIgnore] [NotifyOnReset] public bool ShowStatusBar => ShowMenu && StatusBar || ShowCommandBar && StatusBar;

        [JsonIgnore] public double BackButtonWidth { get; set; }

        [JsonIgnore]
        public bool NotDeferred
        {
            get => _notDeferred;
            set => Set(ref _notDeferred, value, MethodBase.GetCurrentMethod().Name);
        }

        [JsonIgnore] private IServiceProvider ServiceProvider { get; }

        [JsonIgnore] public ObservableCollection<string> AllFonts { get; protected set; }

        [JsonIgnore]
        public IEnumerable<float> AllFontSizes { get; } =
            new float[] {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};

        [JsonIgnore] public Action<double> AfterTintOpacityChanged { get; set; }

        [JsonIgnore] public ObservableCollection<DefaultLanguageModel> DefaultLanguages { get; protected set; }

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

        [JsonIgnore]
        protected virtual SettingsModel<TStorageFile, TStream> Model { get; set; }

        // Not Ignored
        [NotifyOnReset]
        public string CustomThemeId
        {
            get => Model.CustomThemeId;
            set => Model.CustomThemeId = value;
        }

        [NotifyOnReset]
        public bool PasteTextOnly
        {
            get => Model.PasteTextOnly;
            set => Model.PasteTextOnly = value;
        }

        [NotifyOnReset]
        public string DefaultFont
        {
            get => Model.DefaultFont;
            set => Model.DefaultFont = value;
        }

        [NotifyOnReset]
        public string DefaultRtfFont
        {
            get => Model.DefaultRtfFont;
            set => Model.DefaultRtfFont = value;
        }

        [NotifyOnReset]
        public bool SpellCheck
        {
            get => Model.SpellCheck;
            set => Model.SpellCheck = value;
        }

        [NotifyOnReset]
        public bool WordWrap
        {
            get => Model.WordWrap;
            set => Model.WordWrap = value;
        }

        [NotifyOnReset]
        public bool RtfSpellCheck
        {
            get => Model.RtfSpellCheck;
            set => Model.RtfSpellCheck = value;
        }

        [NotifyOnReset]
        public bool RtfWordWrap
        {
            get => Model.RtfWordWrap;
            set => Model.RtfWordWrap = value;
        }

        [NotifyOnReset]
        public bool UseAcrylic
        {
            get => Model.UseAcrylic;
            set => Model.UseAcrylic = value;
        }

        [NotifyOnReset]
        public float DefaultFontRtfSize
        {
            get => Model.DefaultFontRtfSize;
            set => Model.DefaultFontRtfSize = value;
        }

        [NotifyOnReset]
        public float DefaultFontSize
        {
            get => Model.DefaultFontSize;
            set => Model.DefaultFontSize = value;
        }

        [NotifyOnReset]
        public bool AutoSave
        {
            get => Model.AutoSave;
            set => Model.AutoSave = value;
        }

        [NotifyOnReset]
        public int AutoSaveFrequency
        {
            get => Model.AutoSaveFrequency;
            set => Model.AutoSaveFrequency = value;
        }

        [NotifyOnReset]
        public string DefaultFileType
        {
            get => Model.DefaultFileType;
            set => Model.DefaultFileType = value;
        }

        [NotifyOnReset]
        public string DefaultEncoding
        {
            get => Model.DefaultEncoding;
            set => Model.DefaultEncoding = value;
        }

        [NotifyOnReset]
        public string DefaultMode
        {
            get => Model.DefaultMode;
            set
            {
                if (Model.DefaultMode != value)
                {
                    Model.DefaultMode = CurrentMode = value;
                }
            }
        }

        [NotifyOnReset]
        public bool AcknowledgeFontSelectionChange
        {
            get => Model.AcknowledgeFontSelectionChange;
            set => Model.AcknowledgeFontSelectionChange = value;
        }

        [NotifyOnReset]
        public bool EnableGoogleSearch
        {
            get => Model.EnableGoogleSearch;
            set => Model.EnableGoogleSearch = value;
        }

        public abstract string DefaultTextForegroundColor { get; set; }


        // Helper Methods
        private void StatusTimerCallback(object state)
        {
            Set(ref _statusText, CurrentViewModel.FilePath, nameof(StatusText));
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
        }

        public void NotifyAll()
        {
            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CustomAttributes.Any(ca => ca.AttributeType == typeof(NotifyOnResetAttribute)))
                .ToList().ForEach(
                    info => { OnPropertyChanged(info.Name); });
        }

        public abstract string GetTranslation(string resourceName);
        public abstract Task ExportSettings();
        public abstract Task ImportSettings();
        public abstract void ResetSettings();
        public abstract void LaunchUri(Uri uri);
    }
}