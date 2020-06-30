using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace QuickPad.UI.Theme
{
    public class VisualTheme : INotifyPropertyChanged
    {
        public static readonly Color DarkColor = 
            Color.FromArgb(255, 28, 28, 28);
        public static readonly Color LightColor = Colors.White;
        private Color? _foreground;

        public Color DefaultTextForegroundColor 
        {
            get
            {
                if (!_foreground.HasValue)
                {
                    return (Theme == ElementTheme.Dark) ? VisualTheme.LightColor : VisualTheme.DarkColor;
                }

                return _foreground.Value;
            }
            set
            {
                _foreground = value;

                OnPropertyChanged();
            }
        }

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
        public Brush BackgroundAcrylicAccent
        {
            get;
            set;
        }

        public override string ToString()
        {
            return FriendlyName;
        }


        public void UpdateTintOpacity(double to)
        {
            ((AcrylicBrush) BackgroundAcrylicBrush).TintOpacity = to;
            ((AcrylicBrush) BackgroundAcrylicBrush2).TintOpacity = to + .15;
            ((AcrylicBrush)BackgroundAcrylicAccent).TintOpacity = to - .25;
            ((AcrylicBrush) InAppAcrylicBrush).TintOpacity = to;
        }

        public void UpdateBaseBackground(object sender, ThemeChangedEventArgs e)
        {
            BaseThemeBackgroundBrush = 
                Application.Current.RequestedTheme == ApplicationTheme.Dark 
                    ? new SolidColorBrush(Colors.Black) 
                    : new SolidColorBrush(Colors.White);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}