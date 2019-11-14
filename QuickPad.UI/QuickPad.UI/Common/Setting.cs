using System;

namespace QuickPad.UI.Common
{
    public class Setting
    {
        public Action<double> AfterTintOpacityChanged { get; set; }
        public string CustomThemeId { get; set; }
        public double BackgroundTintOpacity { get; set; }
    }
}