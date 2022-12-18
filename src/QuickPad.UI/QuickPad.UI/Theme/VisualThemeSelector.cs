using Microsoft.Extensions.DependencyInjection;
using QuickPad.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace QuickPad.UI.Theme
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

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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
            _themes.Add(BuildTheme("yellow", "ThemeYellowName", true, Color.FromArgb(255, 250, 242, 0)));
            _themes.Add(BuildTheme("orange", "ThemeOrangeNAme", true, Color.FromArgb(255, 236, 102, 0)));
            _themes.Add(BuildTheme("lettuce", "ThemeLettuceName", true, Color.FromArgb(255, 177, 234, 175)));
            _themes.Add(BuildTheme("rosegold", "ThemeRoseGoldName", true, Color.FromArgb(255, 253, 220, 215)));
            _themes.Add(BuildTheme("apricot", "ThemeApricotName", true, Color.FromArgb(255, 255, 202, 175)));
            _themes.Add(BuildTheme("mediumpurple", "ThemeMediumPurpleName", true, Color.FromArgb(255, 147, 112, 219)));
            _themes.Add(BuildTheme("sizzlingred", "ThemeSizzlingRedName", true, Color.FromArgb(255, 240, 93, 94)));
            _themes.Add(BuildTheme("persimmon", "ThemePersimmonName", true, Color.FromArgb(255, 244, 93, 1)));
            _themes.Add(BuildTheme("camel", "ThemeCamelName", true, Color.FromArgb(255, 199, 162, 124)));
            _themes.Add(BuildTheme("middleblue", "ThemeMiddleBlueName", true, Color.FromArgb(255, 126, 212, 230)));
            _themes.Add(BuildTheme("bronze", "ThemeBronzeName", true, Color.FromArgb(255, 213, 137, 54)));
            _themes.Add(BuildTheme("mintcream", "ThemeMintCreamName", true, Color.FromArgb(255, 247, 255, 247)));

            //Custom dark themes:
            _themes.Add(BuildTheme("cobalt", "ThemeCobaltName", false, Color.FromArgb(255, 38, 44, 255)));
            _themes.Add(BuildTheme("leaf", "ThemeLeafName", false, Color.FromArgb(255, 56, 111, 54)));
            _themes.Add(BuildTheme("crimson", "ThemeCrimsonName", false, Color.FromArgb(255, 149, 0, 39)));
            _themes.Add(BuildTheme("darksienna", "ThemeDarkSiennaName", false, Color.FromArgb(255, 46, 15, 21)));
            _themes.Add(BuildTheme("iron", "ThemeIronName", false, Color.FromArgb(255, 72, 73, 75)));
            _themes.Add(BuildTheme("blackcoral", "ThemeBlackCoralName", false, Color.FromArgb(255, 62, 92, 118)));
            _themes.Add(BuildTheme("hibiscus", "ThemeHibiscusName", false, Color.FromArgb(255, 169, 56, 86)));
            _themes.Add(BuildTheme("maximumpurple", "ThemeMaximumPurpleName", false, Color.FromArgb(255, 125, 56, 125)));
            _themes.Add(BuildTheme("darkspringgreen", "ThemeDarkSpringGreenName", false, Color.FromArgb(255, 4, 114, 77)));
            _themes.Add(BuildTheme("bluemunsell", "ThemeBlueMunsellName", false, Color.FromArgb(255, 44, 140, 153)));
            _themes.Add(BuildTheme("cedarchest", "ThemeCedarChestName", false, Color.FromArgb(255, 192, 87, 70)));
            _themes.Add(BuildTheme("raisinblack", "ThemeRaisinBlackName", false, Color.FromArgb(255, 33, 39, 56)));
            _themes.Add(BuildTheme("rust", "ThemeRustName", false, Color.FromArgb(255, 164, 66, 0)));
        }

        private VisualTheme BuildTheme(string themeId, string nameResKey, bool lightTheme, Color accentColor)
        {
            var backgroundMicaBrush1 = new SolidColorBrush
            {
                Color = accentColor,
                Opacity = 0.10
            };
            
            var backgroundMicaBrush2 = new SolidColorBrush
            {
                Color = accentColor,
                Opacity = 0.25
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
                SolidBackgroundBrush = new SolidColorBrush(accentColor),
                BackgroundMicaBrush1 = backgroundMicaBrush1,
                BackgroundMicaBrush2 = backgroundMicaBrush2,
                PreviewBrush = new SolidColorBrush(accentColor),
                BaseThemeBackgroundBrush = etheme == ElementTheme.Dark
                    ? new SolidColorBrush(Color.FromArgb(255, 39, 39, 39))
                    : new SolidColorBrush(Color.FromArgb(255, 249, 249, 249))
            };

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