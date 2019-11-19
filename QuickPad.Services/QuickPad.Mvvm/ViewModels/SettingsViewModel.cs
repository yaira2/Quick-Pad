using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Windows.Management.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using QuickPad.Mvvm.Models;

namespace QuickPad.Mvvm.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        private bool _showSettings;
        private double _defaultFontSize = 14.0;
        private bool _useAcrylic = true;
        private bool _wordWrap = true;
        private bool _spellCheck = true;
        private string _defaultFont = "Times New Roman";
        private DefaultLanguageModel _defaultLanguage = new DefaultLanguageModel();
        private bool _pasteTextOnly = true;
        private double _backgroundTintOpacity = 0.75;
        private string _customThemeId;
        private Action<double> _afterTintOpacityChanged;

        public SettingsViewModel(ILogger<SettingsViewModel> logger) : base(logger)
        {
            AllFonts = new ObservableCollection<FontFamilyModel>(
                CanvasTextFormat.GetSystemFontFamilies()
                    .OrderBy(font => font)
                    .Select(font => new FontFamilyModel(font)));

            var roamingSettings =
                Windows.Storage.ApplicationData.Current.RoamingSettings;

            var json = roamingSettings.Values["JSON"] as string ?? string.Empty;

            var settings = JsonConvert.DeserializeObject<SettingsViewModel>(json,
                new JsonSerializerSettings
                    {ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor});

            if(settings != null)
            {
                GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList()
                    .ForEach(pi =>
                        {
                            if (!pi.CustomAttributes.Any())
                            {
                                pi.SetValue(this, pi.GetValue(settings));
                            }
                        });
            }

            PropertyChanged += (sender, args) =>
            {
                json = JsonConvert.SerializeObject(this, Formatting.Indented);
                roamingSettings.Values["JSON"] = json;
            };
        }

        private SettingsViewModel() : base(null) { }

        [JsonIgnore]
        public ObservableCollection<FontFamilyModel> AllFonts { get; }

        [JsonIgnore]
        public Action<double> AfterTintOpacityChanged
        {
            get => _afterTintOpacityChanged;
            set => Set(ref _afterTintOpacityChanged, value);
        }

        public string CustomThemeId
        {
            get => _customThemeId;
            set => Set(ref _customThemeId, value);
        }

        public double BackgroundTintOpacity
        {
            get => _backgroundTintOpacity;
            set
            {
                Set(ref _backgroundTintOpacity, value);
                AfterTintOpacityChanged?.Invoke(value);
            }
        }

        public bool PasteTextOnly
        {
            get => _pasteTextOnly;
            internal set => Set(ref _pasteTextOnly, value);
        }

        public DefaultLanguageModel DefaultLanguage
        {
            get => _defaultLanguage;
            set => Set(ref _defaultLanguage, value);
        }

        public string DefaultFont
        {
            get => _defaultFont;
            set => Set(ref _defaultFont, value);
        }

        public bool SpellCheck
        {
            get => _spellCheck;
            set => Set(ref _spellCheck, value);
        }

        public bool WordWrap
        {
            get => _wordWrap;
            set => Set(ref _wordWrap, value);
        }

        public bool UseAcrylic     
        {
            get => _useAcrylic;
            set => Set(ref _useAcrylic, value);
        }

        public double DefaultFontSize
        {
            get => _defaultFontSize;
            set => Set(ref _defaultFontSize, value);
        }

        public bool ShowSettings
        {
            get => _showSettings;
            set => Set(ref _showSettings, value);
        }
    }
}
