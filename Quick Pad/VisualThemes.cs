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
            //White Theme
            var whiteBrush = GetConfiguredAcrylicBrush();
            whiteBrush.FallbackColor = Colors.White;
            whiteBrush.TintColor = Colors.White;
            var whiteTheme = new VisualTheme
            {
                ThemeId = "1",
                FriendlyName = "White",
                Theme = ElementTheme.Light,
                BackgroundBrush = whiteBrush,
            };
            _themes.Add(whiteTheme);

            //Black Theme
            var blackBrush = GetConfiguredAcrylicBrush();
            blackBrush.FallbackColor = Colors.Black;
            blackBrush.TintColor = Colors.Black;
            var blackTheme = new VisualTheme
            {
                ThemeId = "2",
                FriendlyName = "Black",
                Theme = ElementTheme.Dark,
                BackgroundBrush = blackBrush,
            };
            _themes.Add(blackTheme);

            //Light Green Theme
            var lgreenBrush = GetConfiguredAcrylicBrush();
            lgreenBrush.FallbackColor = Color.FromArgb(255, 188, 245, 188);
            lgreenBrush.TintColor = Color.FromArgb(255, 144, 238, 144);
            var lgreenTheme = new VisualTheme
            {
                ThemeId = "2",
                FriendlyName = "Lime",
                Theme = ElementTheme.Light,
                BackgroundBrush = lgreenBrush,
            };
            _themes.Add(lgreenTheme);

            //Salmon Theme
            var salmonBrush = GetConfiguredAcrylicBrush();
            salmonBrush.FallbackColor = Color.FromArgb(255, 255, 179, 156);
            salmonBrush.TintColor = Color.FromArgb(255, 255, 179, 156);
            var salmonTheme = new VisualTheme
            {
                ThemeId = "3",
                FriendlyName = "Salmon",
                Theme = ElementTheme.Light,
                BackgroundBrush = salmonBrush,
            };
            _themes.Add(salmonTheme);

            //Cobalt Theme
            var cobaltBrush = GetConfiguredAcrylicBrush();
            cobaltBrush.FallbackColor = Color.FromArgb(255, 0, 71, 171);
            cobaltBrush.TintColor = Color.FromArgb(255, 0, 71, 171);
            var cobaltTheme = new VisualTheme
            {
                ThemeId = "4",
                FriendlyName = "Cobalt",
                Theme = ElementTheme.Dark,
                BackgroundBrush = cobaltBrush,
            };
            _themes.Add(cobaltTheme);

        }
        private AcrylicBrush GetConfiguredAcrylicBrush()
        {
            AcrylicBrush brush = new AcrylicBrush();
            brush.BackgroundSource = AcrylicBackgroundSource.HostBackdrop;
            brush.TintOpacity = .7d;
            return brush;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class VisualTheme
    {
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
        public ElementTheme Theme { get; set; }
        public Brush BackgroundBrush
        {
            get;
            set;
        }
        public Brush BackgroundBrush2
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