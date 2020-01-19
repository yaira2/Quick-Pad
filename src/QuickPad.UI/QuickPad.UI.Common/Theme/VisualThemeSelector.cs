using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.UI.Common.Theme
{
    /// <summary>
    /// </summary>
    public class VisualThemeSelector : IVisualThemeSelector
    {
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse();
        private const string LIGHT_KEY = "light";
        private const string DARK_KEY = "dark";
        private readonly WindowsSettingsViewModel _settingsViewModel;
        private readonly List<VisualTheme> _themes;
        private ThemeChangedEventHandler _themeChanged;

        public static IVisualThemeSelector Current => ServiceProvider?.GetService<IVisualThemeSelector>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event ThemeChangedEventHandler ThemeChanged
        {
            add
            {
                _themeChanged = value;
                NotifyThemeChanged();
            }
            remove
            {
                if (value != null) _themeChanged -= value;
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

        public UISettings SystemUI
        {
            get;
        }

        public static IServiceProvider ServiceProvider { get; private set; }
        public ResourceDictionary Resources { get; }

        public VisualThemeSelector(
            IServiceProvider serviceProvider
            , WindowsSettingsViewModel settingsViewModel
            , ResourceDictionary resources)
        {
            ServiceProvider = serviceProvider;
            Resources = resources;

            _settingsViewModel = settingsViewModel;
            _settingsViewModel.PropertyChanged += SettingsViewModelOnPropertyChanged;

            _themes = new List<VisualTheme>();

            Fill();

            var source = new CollectionViewSource() { Source = _themes };
            ThemesView = source.View;
            ThemesView.CurrentChanged += ThemesView_CurrentChanged;

            //
            SystemUI = new UISettings();
            SystemUI.ColorValuesChanged += SystemThemeChanged;

            //Select here from settings:
            if (string.IsNullOrWhiteSpace(_settingsViewModel.CustomThemeId))
            {
                ThemesView.MoveCurrentToFirst();
            }
            else
            {
                SelectFromId(_settingsViewModel.CustomThemeId);
            }
            //
        }

        private void SettingsViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindowsSettingsViewModel.CustomThemeId) &&
                CurrentItem.ThemeId != _settingsViewModel.CustomThemeId)
            {
                SelectFromId(_settingsViewModel.CustomThemeId);
            }
        }

        private async void SystemThemeChanged(UISettings sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ThemesView_CurrentChanged(null, null);
            });
        }

        private void ThemesView_CurrentChanged(object sender, object e)
        {
            if (!(ThemesView.CurrentItem is VisualTheme theme)) return;

            if (theme.ThemeId == CurrentItem?.ThemeId) return;

            SettingsItem = theme;
            _settingsViewModel.CustomThemeId = theme.ThemeId;

            switch (theme.Kind)
            {
                case VisualThemeKind.System:
                {
                    var systemTheme = GetSystemTheme();
                    var darkTheme = (systemTheme.HasValue && !systemTheme.Value);
                    CurrentItem = GetThemeFromId((darkTheme) ? DARK_KEY : LIGHT_KEY);
                    break;
                }
                case VisualThemeKind.Random:
                {
                    var customThemes = new List<VisualTheme>(_themes.Where(x => x.Kind == VisualThemeKind.Custom));
                    var random = new Random();
                    var index = random.Next(0, customThemes.Count);
                    var luckyOne = customThemes[index];
                    CurrentItem = luckyOne;
                    break;
                }
                default:
                    CurrentItem = theme;
                    break;
            }

            _settingsViewModel.DefaultTextForegroundBrush = 
                new SolidColorBrush(CurrentItem.DefaultTextForegroundColor);

            RaisePropertyChanged(nameof(SettingsItem));
            RaisePropertyChanged(nameof(CurrentItem));
            NotifyThemeChanged();
        }

        private void NotifyThemeChanged()
        {
            _themeChanged?.Invoke(this
                , new ThemeChangedEventArgs(CurrentItem, SettingsItem));
            _settingsViewModel.NotifyThemeChanged();
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
            PropertyChanged?.Invoke(this
                , new PropertyChangedEventArgs(propertyName));
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

            //System Accent Color
            _themes.Add(BuildTheme("accent", "ThemeAccentName", false, (Color)Resources["SystemAccentColor"]));

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
            var backgroundAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = _settingsViewModel.BackgroundTintOpacity,
            };

            var backgroundAcrylic2 = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = (_settingsViewModel.BackgroundTintOpacity + .15) > 1 ? 1 : _settingsViewModel.BackgroundTintOpacity + .15
            };

            var backgroundAcrylicAccent = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = (_settingsViewModel.BackgroundTintOpacity + - .25) < 0 ? 0 : _settingsViewModel.BackgroundTintOpacity - .25
            };

            var inAppAcrylic = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.Backdrop,
                FallbackColor = accentColor,
                TintColor = accentColor,
                TintOpacity = (_settingsViewModel.BackgroundTintOpacity + .05) > 1 ? 1 : _settingsViewModel.BackgroundTintOpacity + .05
            };

            var etheme = (lightTheme) ? ElementTheme.Light : ElementTheme.Dark;

            var descriptionResKey = (lightTheme) 
                ? "ThemeGeneralLightDescription" 
                : "ThemeGeneralDarkDescription";

            var theme = new VisualTheme
            {
                ThemeId = themeId,
                FriendlyName = _loader.GetString(nameResKey),
                Description = _loader.GetString(descriptionResKey),
                Theme = etheme,
                BackgroundAcrylicBrush = backgroundAcrylic,
                BackgroundAcrylicBrush2 = backgroundAcrylic2,
                BackgroundAcrylicAccent = backgroundAcrylicAccent,
                InAppAcrylicBrush = inAppAcrylic,
                SolidBackgroundBrush = new SolidColorBrush(accentColor),
                PreviewBrush = new SolidColorBrush(accentColor),
                BaseThemeBackgroundBrush = etheme == ElementTheme.Dark
                    ? new SolidColorBrush(Color.FromArgb(255, 28, 28, 28))
                    : new SolidColorBrush(Colors.White),
            };

            _settingsViewModel.AfterTintOpacityChanged += theme.UpdateTintOpacity;

            return theme;
        }

        private VisualTheme GetThemeFromId(string id)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            
            var match = ThemesView.OfType<VisualTheme>().FirstOrDefault(
                x => comparer.Equals(x.ThemeId, id));

            return match;
        }

        private static bool? GetSystemTheme()
        {
            var defaultTheme = new UISettings();

            var uiTheme = defaultTheme.GetColorValue(UIColorType.Background).ToString();

            switch (uiTheme)
            {
                case "#FF000000":
                    return false;
                case "#FFFFFFFF":
                    return true;
                default:
                    return null;
            }
        }
    }
}
