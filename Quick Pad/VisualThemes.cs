using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace QuickPad
{
    public class VisualThemeSelector : INotifyPropertyChanged
    {
        private ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse();
        private const string LIGHT_KEY = "light";
        private const string DARK_KEY = "dark";
        private static VisualThemeSelector _default;
        public static VisualThemeSelector Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new VisualThemeSelector();
                    Setting s = new Setting();
                    _default.SelectFromId(s.CustomThemeId);
                }
                return _default;
            }
        }

        private Setting _setting = App.QSetting;

        private List<VisualTheme> _themes;
        private ThemeChangedEventHandler _themeChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event ThemeChangedEventHandler ThemeChanged
        {
            add
            {
                _themeChanged += value;
                NotifyThemeChanged();
            }
            remove
            {
                _themeChanged -= value;
            }
        }

        public ICollectionView ThemesView
        {
            get;
            set;
        }
        public VisualTheme SettingsItem
        {
            get;
            set;
        }
        public VisualTheme CurrentItem
        {
            get;
            private set;
        }

        public VisualThemeSelector()
        {
            _themes = new List<VisualTheme>();

            Fill();

            var source = new CollectionViewSource() { Source = _themes };
            ThemesView = source.View;
            ThemesView.CurrentChanged += ThemesView_CurrentChanged;

            //Select here from settings:
            ThemesView.MoveCurrentToFirst();
            //
        }
        private void ThemesView_CurrentChanged(object sender, object e)
        {
            if (ThemesView.CurrentItem is VisualTheme theme)
            {
                SettingsItem = theme;
                if (theme.Kind ==VisualThemeKind.System)
                {
                    var systemTheme = GetSystemTheme();
                    bool darkTheme = (systemTheme.HasValue && !systemTheme.Value);
                    CurrentItem = GetThemeFromId((darkTheme) ? DARK_KEY : LIGHT_KEY);
                }
                else if(theme.Kind == VisualThemeKind.Random)
                {
                    List<VisualTheme> customThemes = new List<VisualTheme>(_themes.Where(x => x.Kind == VisualThemeKind.Custom));
                    Random random = new Random();
                    var index = random.Next(0, customThemes.Count);
                    var luckyOne = customThemes[index];
                    CurrentItem = luckyOne;
                }
                else
                {
                    CurrentItem = theme;
                }
                RaisePropertyChanged(nameof(SettingsItem));
                RaisePropertyChanged(nameof(CurrentItem));
                NotifyThemeChanged();
            }
        }
        private void NotifyThemeChanged()
        {
            _themeChanged?.Invoke(this, new ThemeChangedEventArgs(CurrentItem, SettingsItem));
        }
        private void SelectFromId(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var match = GetThemeFromId(id);
                if (match != null)
                {
                    ThemesView.MoveCurrentTo(match);
                }
            }
        }
        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Fill()
        {
            /*Priority Themes*/
            //Default:
            var defPreview = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1), };
            defPreview.GradientStops.Add(new GradientStop { Color = VisualTheme.DarkColor, Offset = .5d });
            defPreview.GradientStops.Add(new GradientStop { Color = VisualTheme.LightColor, Offset = .5d });
            var def = new VisualTheme
            {
                ThemeId = "default",
                Description = _loader.GetString("ThemeSystemDescription"),
                FriendlyName = _loader.GetString("ThemeSystemName"),
                Theme = ElementTheme.Default,
                Kind = VisualThemeKind.System,
                PreviewBrush = defPreview
            };
            _themes.Add(def);
            //Light
            _themes.Add(BuildTheme(LIGHT_KEY, "ThemeLightName", true, VisualTheme.LightColor));
            //Dark
            _themes.Add(BuildTheme(DARK_KEY, "ThemeDarkName", false, VisualTheme.DarkColor));
            //Random:
            var rdmPreview = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1), };
            rdmPreview.GradientStops.Add(new GradientStop { Color = Colors.Red, Offset = 0d });
            rdmPreview.GradientStops.Add(new GradientStop { Color = Colors.Yellow, Offset = .25d });
            rdmPreview.GradientStops.Add(new GradientStop { Color = Colors.LightGreen, Offset = .50d });
            rdmPreview.GradientStops.Add(new GradientStop { Color = Colors.Teal, Offset = .75d });
            rdmPreview.GradientStops.Add(new GradientStop { Color = Colors.Violet, Offset = 1d });
            var rdm = new VisualTheme
            {
                ThemeId = "random",
                FriendlyName = _loader.GetString("ThemeRandomName"),
                Description = _loader.GetString("ThemeRandomDescription"),
                Theme = ElementTheme.Default,
                Kind = VisualThemeKind.Random,
                PreviewBrush = rdmPreview
            };
            _themes.Add(rdm);
            //Custom light themes:
            _themes.Add(BuildTheme("chick", "ThemeChickName", true, Color.FromArgb(255, 254, 255, 177)));
            _themes.Add(BuildTheme("lettuce", "ThemeLettuceName", true, Color.FromArgb(255, 177, 234, 175)));
            _themes.Add(BuildTheme("rosegold", "ThemeRoseGoldName", true, Color.FromArgb(255, 253, 220, 215)));
            //Custom dark themes:
            _themes.Add(BuildTheme("cobalt", "ThemeCobaltName", false, Color.FromArgb(255, 0, 71, 171)));
            _themes.Add(BuildTheme("leaf", "ThemeLeafName", false, Color.FromArgb(255, 56, 111, 54)));
            _themes.Add(BuildTheme("crimson", "ThemeCrimsonName", false, Color.FromArgb(255, 149, 0, 39)));
        }
        private VisualTheme BuildTheme(string themeId, string nameResKey, bool lightTheme, Color accentColor)
        {
            AcrylicBrush backgroundAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = _setting.BackgroundTintOpacity,
            };
            AcrylicBrush backgroundAcrylic2 = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = (_setting.BackgroundTintOpacity + .15) > 1 ? 1 : _setting.BackgroundTintOpacity + .15 
            };
            AcrylicBrush inAppAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = (_setting.BackgroundTintOpacity + .05) > 1 ? 1 : _setting.BackgroundTintOpacity + .05
            };
            var etheme = (lightTheme) ? ElementTheme.Light : ElementTheme.Dark;
            string descriptionResKey = (lightTheme) ? "ThemeGeneralLightDescription" : "ThemeGeneralDarkDescription";
            var theme = new VisualTheme
            {
                ThemeId = themeId,
                FriendlyName = _loader.GetString(nameResKey),
                Description = _loader.GetString(descriptionResKey),
                Theme = etheme,
                BackgroundAcrylicBrush = backgroundAcrylic,
                BackgroundAcrylicBrush2 = backgroundAcrylic2,
                InAppAcrylicBrush = inAppAcrylic,
                SolidBackgroundBrush = new SolidColorBrush(accentColor),
                PreviewBrush = new SolidColorBrush(accentColor),
            };
            theme.BaseThemeBackgroundBrush = etheme == ElementTheme.Dark ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);
            _setting.afterTintOpacityChanged += theme.UpdateTintOpacity;
            return theme;
        }
        private VisualTheme GetThemeFromId(string id)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var match = ThemesView.Select(x => x as VisualTheme).FirstOrDefault(x => comparer.Equals(x.ThemeId, id));
            return match;
        }
        private static bool? GetSystemTheme()
        {
            var DefaultTheme = new UISettings();
            var uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
            {
                return false;
            }
            else if (uiTheme == "#FFFFFFFF")
            {
                return true;
            }
            return null;
        }
    }
    public class VisualTheme
    {
        public static readonly Color DarkColor = Color.FromArgb(255, 28, 28, 28);
        public static readonly Color LightColor = Colors.White;
        private Color? _foreground;

        public string ThemeId
        {
            get;
            set;
        }
        public string FriendlyName
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public ElementTheme Theme
        {
            get;
            set;
        }
        public VisualThemeKind Kind
        {
            get;
            set;
        } = VisualThemeKind.Custom;
        public Brush BackgroundAcrylicBrush
        {
            get;
            set;
        }
        public Brush BackgroundAcrylicBrush2
        {
            get;
            set;
        }
        public Brush PreviewBrush
        {
            get;
            set;
        }
        public Brush InAppAcrylicBrush
        {
            get;
            set;
        }
        public Brush SolidBackgroundBrush
        {
            get;
            set;
        }
        public Brush BaseThemeBackgroundBrush
        {
            get;
            set;
        }
        public Color DefaultTextForeground
        {
            get
            {
                if (!_foreground.HasValue)
                {
                    return (Theme == ElementTheme.Dark) ? LightColor : DarkColor;
                }
                return _foreground.Value;
            }
            set
            {
                _foreground = value;
            }
        }
        public override string ToString()
        {
            return FriendlyName;
        }


        public void UpdateTintOpacity(double to)
        {
            (BackgroundAcrylicBrush as AcrylicBrush).TintOpacity = to;
            (BackgroundAcrylicBrush2 as AcrylicBrush).TintOpacity = to + .15;
            (InAppAcrylicBrush as AcrylicBrush).TintOpacity = to;
        }

        public void UpdateBaseBackground(object sender, ThemeChangedEventArgs e)
        {
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                BaseThemeBackgroundBrush = new SolidColorBrush(Colors.Black);
            }
            else
            {
                BaseThemeBackgroundBrush = new SolidColorBrush(Colors.White);
            }
        }
    }
    public enum VisualThemeKind
    {
        System,
        Random,
        Custom,
    }

    public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);
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