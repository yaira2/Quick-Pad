using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using QuickPad.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickPad.MVVM
{
    public class SettingsViewModel : ViewModel
    {
        public SettingsViewModel() : base(null) { }

        public SettingsViewModel(ILogger logger) : base(logger)
        {
            AllFonts = new ObservableCollection<FontFamilyModel>(CanvasTextFormat.GetSystemFontFamilies().OrderBy(font => font).Select(font => new FontFamilyModel(font)));
        }

        public Action<double> AfterTintOpacityChanged { get; set; }
        public string CustomThemeId { get; set; }
        public double BackgroundTintOpacity { get; set; }
        public bool PasteTextOnly { get; internal set; }
        public ObservableCollection<FontFamilyModel> AllFonts { get; }
        public DefaultLanguageModel DefaultLanguage { get; set; }
    }
}
