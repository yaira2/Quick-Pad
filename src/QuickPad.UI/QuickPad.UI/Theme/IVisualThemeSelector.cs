using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace QuickPad.UI.Theme
{
    public interface IVisualThemeSelector : INotifyPropertyChanged
    {
        event ThemeChangedEventHandler ThemeChanged;
        ICollectionView ThemesView { get; set; }
        VisualTheme SettingsItem { get; set; }
        VisualTheme CurrentItem { get; }
        UISettings SystemUI { get; }
        ResourceDictionary Resources { get; }
        void RaisePropertyChanged([CallerMemberName]string propertyName = "");
    }
}