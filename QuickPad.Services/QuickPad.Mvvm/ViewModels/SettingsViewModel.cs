using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Management.Core;
using Windows.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using QuickPad.Mvvm.Models;
using Windows.Globalization;

namespace QuickPad.Mvvm.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        readonly ApplicationDataContainer _roamingSettings;

        public SettingsViewModel(ILogger<SettingsViewModel> logger) : base(logger)
        {
            AllFonts = new ObservableCollection<string>(
                CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font));
            
            _roamingSettings = ApplicationData.Current.RoamingSettings;

            var supportedLang = ApplicationLanguages.ManifestLanguages;
            DefaultLanguages = new ObservableCollection<DefaultLanguageModel>();
            foreach (var lang in supportedLang)
            {
                DefaultLanguages.Add(new DefaultLanguageModel(lang));
            }
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
            new double[] {4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72};

        [JsonIgnore]
        public Action<double> AfterTintOpacityChanged { get; set; }

        [JsonIgnore]
        public ObservableCollection<DefaultLanguageModel> DefaultLanguages { get; }

        public string CustomThemeId
        {
            get => Get((string)null);
            set => Set(value);
        }

        public double BackgroundTintOpacity
        {
            get => Get(0.75);
            set
            {
                Set(value);
                AfterTintOpacityChanged?.Invoke(value);
            }
        }

        public bool PasteTextOnly
        {
            get => Get(true);
            internal set => Set(value);
        }

        public DefaultLanguageModel DefaultLanguage
        {
            get => Get((DefaultLanguageModel)null);
            set => Set(value);
        }

        public string DefaultFont
        {
            get => Get("Consolas");
            set => Set(value);
        }

        public string DefaultRtfFont
        {
            get => Get("Calibri");
            set => Set(value);
        }

        public bool SpellCheck
        {
            get => Get(false);
            set => Set(value);
        }

        public bool WordWrap
        {
            get => Get(false);
            set => Set(value);
        }

        public bool RtfSpellCheck
        {
            get => Get(true);
            set => Set(value);
        }

        public bool RtfWordWrap
        {
            get => Get(true);
            set => Set(value);
        }

        public bool UseAcrylic     
        {
            get => Get(false);
            set => Set(value);
        }

        public double DefaultFontRtfSize
        {
            get => Get(12.0);
            set => Set(value);
        }

        public double DefaultFontSize
        {
            get => Get(14.0);
            set => Set(value);
        }

        public bool ShowSettings
        {
            get => Get(false);
            set => Set(value);
        }

        public bool ModeByFileType
        {
            get => Get(false);
            set => Set(value);
        }

        public bool AutoSave
        {
            get => Get(false);
            set => Set(value);
        }

        public int AutoSaveFrequency
        {
            get => Get(10);
            set => Set(value);
        }

        public string DefaultFileType
        {
            get => Get(".txt");
            set => Set(value);
        }

        public string DefaultEncoding
        {
            get => Get("Utf8");
            set => Set(value);
        }

        public string DefaultMode
        {
            get => Get("Default");
            set => Set(value);
        }

        private string _currentMode;
        public string CurrentMode
        {
            get => _currentMode ?? DefaultMode;
            set => Set(ref _currentMode, value);
        }

        public bool ShowMenu => CurrentMode.Equals("Classic Mode", StringComparison.InvariantCultureIgnoreCase);

        public bool ShowCommandBar => CurrentMode.Equals("Default", StringComparison.InvariantCultureIgnoreCase);
    }
}
