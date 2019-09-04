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
            _themeChanged?.Invoke(this, new ThemeChangedEventArgs(CurrentItem.Theme));
        }

        private void Fill()
        {
            _themes.Add(BuildTheme("0", "White", true, VisualTheme.LightColor));
            _themes.Add(BuildTheme("1", "Black", false, VisualTheme.DarkColor));
            _themes.Add(BuildTheme("2", "Light Green", true, Color.FromArgb(255, 144, 238, 144)));
            _themes.Add(BuildTheme("3", "Cobalt", false, Color.FromArgb(255, 0, 71, 171)));
        }
        private VisualTheme BuildTheme(string themeId, string name, bool lightTheme, Color accentColor)
        {
            AcrylicBrush backgroundAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = .7d
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

        private ElementTheme _theme;
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
            get => _theme;
            set
            {
                _theme = value;
                if (DefaultTextForeground == null)
                {
                    DefaultTextForeground = new SolidColorBrush
                        ((_theme == ElementTheme.Dark)
                        ? LightColor
                        : DarkColor);
                }
            }
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
        public Brush DefaultTextForeground
        {
            get;
            set;
        }

        public override string ToString()
        {
            return FriendlyName;
        }
    }

    public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);
    public class ThemeChangedEventArgs : EventArgs
    {
        public ElementTheme Theme{ get; set; }
        public ThemeChangedEventArgs(ElementTheme theme)
        {
            Theme = theme;
        }
    }

}