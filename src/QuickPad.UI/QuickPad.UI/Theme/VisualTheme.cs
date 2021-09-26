using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace QuickPad.UI.Theme
{
    public class VisualTheme : INotifyPropertyChanged
    {
        public static readonly Color DarkColor = Color.FromArgb(255, 39, 39, 39);

        public static readonly Color LightColor = Color.FromArgb(255, 249, 249, 249);

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

        public Brush BackgroundMicaBrush2
        {
            get;
            set;
        }

        public Brush PreviewBrush
        {
            get;
            set;
        }

        public Brush SolidBackgroundBrush
        {
            get;
            set;
        }
        
        public Brush BackgroundMicaBrush1
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


        public void UpdateBaseBackground(object sender, ThemeChangedEventArgs e)
        {
            BaseThemeBackgroundBrush =
                Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? new SolidColorBrush(Color.FromArgb(255, 39, 39, 39))
                    : new SolidColorBrush(Color.FromArgb(255, 249, 249, 249));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}