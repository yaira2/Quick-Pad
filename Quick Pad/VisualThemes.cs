using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace QuickPad
{
    public class VisualThemeSelector : INotifyPropertyChanged
    {
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
                CurrentItem = theme;
                RaisePropertyChanged(nameof(CurrentItem));
                NotifyThemeChanged();
            }
        }
        private void NotifyThemeChanged()
        {
            _themeChanged?.Invoke(this, new ThemeChangedEventArgs(CurrentItem));
        }
        private void SelectFromId(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var comparer = StringComparer.OrdinalIgnoreCase;
                var match = ThemesView.Select(x => x as VisualTheme).FirstOrDefault(x => comparer.Equals(x.ThemeId, id));
                if (match != null)
                {
                    ThemesView.MoveCurrentTo(match);
                }
            }
        }

        private void Fill()
        {
            //Light themes:
            _themes.Add(BuildTheme("light", "Light", true, VisualTheme.LightColor));
            _themes.Add(BuildTheme("chick", "Chick", true, Color.FromArgb(255, 254, 255, 177)));
            _themes.Add(BuildTheme("lettuce", "Lettuce", true, Color.FromArgb(255, 177, 234, 175), .8));
            _themes.Add(BuildTheme("rosegold", "Rose Gold", true, Color.FromArgb(255, 253, 220, 215), .8));
            //Dark themes:
            _themes.Add(BuildTheme("dark", "Dark", false, VisualTheme.DarkColor));
            _themes.Add(BuildTheme("cobalt", "Cobalt", false, Color.FromArgb(255, 0, 71, 171)));
            _themes.Add(BuildTheme("leaf", "Leaf", false, Color.FromArgb(255, 56, 111, 54)));
            _themes.Add(BuildTheme("crimson", "Crimson", false, Color.FromArgb(255, 149, 0, 39)));
        }
        private VisualTheme BuildTheme(string themeId, string name, bool lightTheme, Color accentColor, double tintOpacity=.7)
        {
            AcrylicBrush backgroundAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = tintOpacity,
            };
            AcrylicBrush inAppAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = .85d
            };
            var etheme = (lightTheme) ? ElementTheme.Light : ElementTheme.Dark;
            var theme = new VisualTheme
            {
                ThemeId = themeId,
                FriendlyName = name,
                Theme = etheme,
                BackgroundAcrylicBrush = backgroundAcrylic,
                InAppAcrylicBrush = inAppAcrylic,
                SolidBackgroundBrush = new SolidColorBrush(accentColor),
            };
            return theme;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        public ElementTheme Theme
        {
            get;
            set;
        }
        public Brush BackgroundAcrylicBrush
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
    }

    public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);
    public class ThemeChangedEventArgs : EventArgs
    {
        public VisualTheme VisualTheme { get; set; }
        public ThemeChangedEventArgs(VisualTheme theme)
        {
            VisualTheme = theme;
        }
    }

}