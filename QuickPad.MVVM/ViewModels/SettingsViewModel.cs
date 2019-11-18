using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using QuickPad.Mvvm.Models;

namespace QuickPad.Mvvm.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        public SettingsViewModel() : base(null) { }

        public SettingsViewModel(ILogger logger) : base(logger)
        {
            AllFonts = new ObservableCollection<FontFamilyModel>(
                CanvasTextFormat.GetSystemFontFamilies()
                    .OrderBy(font => font)
                    .Select(font => new FontFamilyModel(font)));
        }

        public Action<double> AfterTintOpacityChanged { get; set; }
        public string CustomThemeId { get; set; }
        public double BackgroundTintOpacity { get; set; } = 50.0;
        public bool PasteTextOnly { get; internal set; } = true;
        public ObservableCollection<FontFamilyModel> AllFonts { get; }
        public DefaultLanguageModel DefaultLanguage { get; set; } = new DefaultLanguageModel();
        public string DefaultFont { get; set; } = "Times New Roman";
        public bool SpellCheck { get; set; } = true;
        public bool WordWrap { get; set; } = true;
        public bool PreventText1ChangeColor { get; set; } = false;
        public double DefaultFontSize { get; set; } = 14.0;
    }
}
