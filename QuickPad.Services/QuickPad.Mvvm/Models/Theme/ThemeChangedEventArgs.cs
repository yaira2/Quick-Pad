using System;

namespace QuickPad.Mvvm.Models.Theme
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public VisualTheme VisualTheme { get; set; }
        public VisualTheme ActualTheme { get; set; }
        public ThemeChangedEventArgs(VisualTheme theme, VisualTheme actualTheme)
        {
            VisualTheme = theme;
            ActualTheme = actualTheme;
        }
    }
}