using Microsoft.AppCenter.Analytics;
using QuickPad.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace QuickPad
{
    public class Setting : INotifyPropertyChanged
    {
        #region Get/Set notification
        public event PropertyChangedEventHandler PropertyChanged;

        public T Get<T>([CallerMemberName]string propertyName = null)
        {
            if (!localSettings.Values.ContainsKey(propertyName))
            {
                    localSettings.Values.Add(propertyName, GetDefaultValue<T>(propertyName));
            }
            if (localSettings.Values[propertyName].GetType() != typeof(T))
            {
                try
                {
                    MigrateSettingFromPreviousVersion<T>(propertyName);
                }
                catch (Exception ex)
                {
                    Analytics.TrackEvent($"Error trying to migrate setting {propertyName}\r\n{ex.Message}");
                }
            }
            return (T)localSettings.Values[propertyName];
        }

        private void MigrateSettingFromPreviousVersion<T>(string propertyName)
        {
            if (localSettings.Values[propertyName].GetType() == typeof(T))
                return;
            object previousSetting = default;
            string conversion;

            #region Setting conversion
            if (propertyName == nameof(LaunchMode))
            {
                conversion = localSettings.Values[propertyName] as string;
                if (conversion == "Default")
                {
                    previousSetting = AvailableModes.Default;
                }
                else if (conversion == "On Top")
                {
                    previousSetting = AvailableModes.OnTop;
                }
                else if (conversion == "Focus Mode")
                {
                    previousSetting = AvailableModes.Focus;
                }
            }
            else if(propertyName == nameof(DefaultEncoding))
            {
                conversion = localSettings.Values[propertyName] as string;
                if (conversion.Equals("UTF-8"))
                {
                    previousSetting = Encoding.UTF8;
                }
                else if(conversion.Equals("UTF-16 BE"))
                {
                    previousSetting = Encoding.UTF16_BE;
                }
                else if(conversion.Equals("UTF-16 LE"))
                {
                    previousSetting = Encoding.UTF16_LE;
                }
                else if (conversion.Equals("UTF-32"))
                {
                    previousSetting = Encoding.UTF32;
                }
            }
            else if (propertyName == nameof(AutoSave))
            {
                //On and Off to boolean
                conversion = localSettings.Values[propertyName] as string;
                if (conversion == "On")
                {
                    previousSetting = true;
                }
                else
                {
                    previousSetting = false;
                }
            }
            else if (propertyName == nameof(WordWrap) ||
                propertyName == nameof(SpellCheck) ||
                propertyName == nameof(ShowBullets) ||
                propertyName == nameof(ShowStrikethroughOption) ||
                propertyName == nameof(ShowAlignLeft) ||
                propertyName == nameof(ShowAlignCenter) ||
                propertyName == nameof(ShowAlignRight) ||
                propertyName == nameof(ShowAlignJustify))
            {
                //Yes and No to boolean
                conversion = localSettings.Values[propertyName] as string;
                if (conversion == "Yes")
                {
                    previousSetting = true;
                }
                else
                {
                    previousSetting = false;
                }
            }
            else if (propertyName == nameof(Theme))
            {
                //Theme string to ElementTheme
                conversion = localSettings.Values[propertyName] as string;
                if (conversion.StartsWith("System"))
                {
                    previousSetting = (int)ElementTheme.Default;
                }
                else
                {
                    previousSetting = (int)Enum.Parse(typeof(ElementTheme), conversion);
                }
            }
            else if (propertyName == nameof(NewUser) || propertyName == nameof(DefaultFontSize))
            {
                conversion = localSettings.Values[propertyName] as string;
                previousSetting = int.Parse(conversion);
            }
            #endregion

            //Remove old setting
            localSettings.Values.Remove(propertyName);
            //Add new setting
            localSettings.Values.Add(propertyName, (T)previousSetting);
        }

        public static T GetDefaultValue<T>(string propertyName)
        {
            PropertyInfo[] properties = typeof(Setting).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name != propertyName)
                    continue;
                var attrs = property.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is DefaultValueAttribute def)
                    {
                        return (T)def.Value;
                    }
                }
            }
#if DEBUG
            throw new Exception("This setting does not have default setting!");
#else
            return default(T);
#endif
        }

        /// <summary>
        /// Set property to setting and after that send a notification to all property that bind to it
        /// </summary>
        /// <typeparam name="T">That property type</typeparam>
        /// <param name="value">A new value of the setting</param>
        /// <param name="propertyName">A name of the property that will update, 
        /// don't have to send it as it automatically get a property name that send it</param>
        /// <returns>Return a boolean, if the setting has been update and notified will return true, otherwise false</returns>
        public bool Set<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (!localSettings.Values.ContainsKey(propertyName))
            {
                localSettings.Values.Add(propertyName, value);
                NotifyPropertyChanged(propertyName);
                return true;
            }
            if (!Equals(localSettings.Values[propertyName], value))
            {
                localSettings.Values[propertyName] = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        #region Settings
        [DefaultValue("en-US")]
        public string AppLanguage
        {
            get => Get<string>();
            set => Set(value);
        }

        [DefaultValue((int)(AvailableModes.Default))]
        public int LaunchMode
        {
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowStatusBar
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(320)]
        public int CompactSizeHeight
        {
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(200)]
        public int CompactSizeWidth
        {
            get => Get<int>();
            set => Set(value);
        }

        /// <summary>
        /// this is to check the default file extension choosen in the save file dialog
        /// </summary>
        [DefaultValue(".rtf")]
        public string DefaultFileType
        {
            get => Get<string>();
            set => Set(value);
        }

        [DefaultValue((int)Encoding.UTF8)]
        public int DefaultEncoding
        {
            get => Get<int>();
            set => Set(value);
        }


        [DefaultValue(true)]
        public bool AutoSave
        {
            get => Get<bool>();
            set
            {
                if (Set(value))
                {
                    afterAutoSaveChanged?.Invoke(value);
                }
            }
        }

        [DefaultValue(10)]
        public int AutoSaveInterval
        {
            get => Get<int>();
            set
            {
                if (Set(value))
                    afterAutoSaveIntervalChanged?.Invoke(value);
            }
        }

        [DefaultValue(true)]
        public bool WordWrap
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool SpellCheck
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(false)]
        public bool AutoPickMode
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(ElementTheme.Default)]
        public ElementTheme Theme
        {
            get => (ElementTheme)Get<int>();
            set
            {
                if (Set((int)value))
                {
                    //Send an event the theme has changed
                    afterThemeChanged?.Invoke(value);
                }
            }
        }

        [DefaultValue("")]
        public string CustomThemeId
        {
            get => Get<string>();
            set => Set(value);
        }
        #endregion

        #region Toolbar setting
        [DefaultValue(true)]
        public bool ShowFont
        {
            get => Get<bool>();
            set => Set(value);
        }
        
        [DefaultValue(true)]
        public bool ShowColor
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowEmoji
        {
            get
            {
                if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                {
                    return false;
                }
                return Get<bool>();
            }

            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowBold
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowItalic
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowUnderline
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowBullets
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowStrikethroughOption
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowAlignLeft
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowAlignCenter
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowAlignRight
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowAlignJustify
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowSizeUp
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(true)]
        public bool ShowSizeDown
        {
            get => Get<bool>();
            set => Set(value);
        }
        #endregion

        #region Font setting
        [DefaultValue("Consolas")]
        public string DefaultFont
        {
            get => Get<string>();
            set => Set(value);
        }

        [DefaultValue("Default")]
        public string DefaultFontColor
        {
            get => Get<string>();
            set
            {
                if (Set(value))
                {
                    Color update;
                    if (value == "Default")
                    {
                        update = new UISettings().GetColorValue(UIColorType.Foreground);
                    }
                    else if (value.StartsWith("#"))
                    {
                        update = Converter.GetColorFromHex(value);
                    }
                    else
                    {
                        update = (Color)XamlBindingHelper.ConvertValue(typeof(Color), value);
                    }
                    afterFontColorChanged?.Invoke(update);
                }
            }
        }

        [DefaultValue(12)]
        public int DefaultFontSize
        {
            get => Get<int>();
            set
            {
                //Hard limit
                if (value > short.MaxValue)
                {
                    value = short.MaxValue;
                }
                else if (value < 4)
                {
                    value = 4;
                }

                if (Set(value))
                {
                    afterFontSizeChanged?.Invoke(value);
                }
            }
        }

        [DefaultValue(1)]
        public int NewFileAutoNumber
        {
            get => Get<int>();
            set => Set(value);
        }
        #endregion

        #region Advanced/Others
        [DefaultValue(true)]
        public bool SendAnalyticsReport
        {
            get => Get<bool>();
            set
            {
                if (Set(value))
                {
                    Analytics.Instance.InstanceEnabled = value;
                }
            }
        }

        [DefaultValue(false)]
        public bool SaveLogWhenCrash
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(false)]
        public bool PreventAppCloseAfterCrash
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(false)]
        public bool PreventText1ChangeColor
        {
            get => Get<bool>();
            set => Set(value);
        }

        [DefaultValue(0.75)]
        public double BackgroundTintOpacity
        {
            get => Get<double>();
            set
            {
                if (Set(value))
                    afterTintOpacityChanged?.Invoke(value);
            }
        }

        [DefaultValue(false)]
        public bool PasteTextOnly
        {
            get => Get<bool>();
            set => Set(value);
        }
        #endregion

        #region Info & Acknowledgement 
        [DefaultValue(0)]
        public int NewUser
        { //Use to track number of launch time. Asking for feedback on 2
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(0)]
        public int TimesUsingFocusMode
        { //Use to track focus mode, if less than 2 times it will show tip, else it won't
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(false)]
        public bool AcknowledgeFontSelectionChange
        { //Use to inform user about font selection has move into command bar 1
            get => Get<bool>();
            set => Set(value);
        }

        public bool NewerThanApril2018Update => ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
        #endregion

        #region Manage
        public void ResetSettings()
        {
            localSettings.Values.Clear();
            Application.Current.Exit();
        }

        public string ExportSetting()
        {
            string appConfig = "";
            appConfig += $"#{Package.Current.DisplayName}\r\n";
            appConfig += $"#{string.Format(ResourceLoader.GetForCurrentView().GetString("VersionFormat"), Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision)}\r\n";
            foreach (var set in localSettings.Values.ToList().OrderBy(i => i.Key))
            {
                appConfig += $"[{set.Value.GetType().Name}]{set.Key}={set.Value}\r\n";
            }
            return appConfig;
        }

        public void ImportSetting(string settings)
        {
            string[] lines = settings.Split("'\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith('#'))
                    continue;
                var infos = line.Split(new char[] { '[', ']', '=' }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    switch (infos[0])
                    {
                        case "String":
                            if (localSettings.Values.ContainsKey(infos[1]))
                                localSettings.Values[infos[1]] = infos[2];
                            else
                                localSettings.Values.Add(infos[1], infos[2]);
                            break;
                        case "Boolean":
                            if (localSettings.Values.ContainsKey(infos[1]))
                                localSettings.Values[infos[1]] = infos[2] == "True";
                            else
                                localSettings.Values.Add(infos[1], infos[2] == "True");
                            break;
                        case "Int32":
                            if (localSettings.Values.ContainsKey(infos[1]))
                                localSettings.Values[infos[1]] = int.Parse(infos[2]);
                            else
                                localSettings.Values.Add(infos[1], int.Parse(infos[2]));
                            break;
                    }
                }
                catch
                {
                    continue;
                }
                //Refresh settings
                NotifyPropertyChanged(infos[1]);
            }
        }

        #endregion

        #region Events when setting change
        public autoSaveChange afterAutoSaveChanged { get; set; }
        public autosaveIntervalChange afterAutoSaveIntervalChanged { get; set; }
        public themeChange afterThemeChanged { get; set; }
        public fontsizeChange afterFontSizeChanged { get; set; }
        public fontcolorChange afterFontColorChanged { get; set; }
        public tintopacityChange afterTintOpacityChanged { get; set; }
        #endregion
    }

    public delegate void autoSaveChange(bool to);
    public delegate void autosaveIntervalChange(int to);
    public delegate void themeChange(ElementTheme to);
    public delegate void fontsizeChange(int to);
    public delegate void fontcolorChange(Color to);
    public delegate void fontnameChange(string to);
    public delegate void tintopacityChange(double to);

    public enum AvailableModes
    {
        Default, //Full UI with toolbar
        OnTop, //Compact overlay
        Focus, //Hide toolbar
        Classic //Like a classic notepad
    }

    public static class Converter
    {
        public static TextWrapping BoolToTextWrap(bool input)
        {
            if (input)
            {
                return TextWrapping.Wrap;
            }
            return TextWrapping.NoWrap;
        }

        /// <summary>
        /// Use to convert on XAML & x:Bind from bool (or Boolean) to Visibility
        /// </summary>
        /// <param name="input">Any boolean input</param>
        /// <returns>Visibility if true return Visible otherwise return Collapsed</returns>
        public static Visibility BoolToVisibility(bool input)
        {
            if (input)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Use to convert on XAML & x:Bind from bool (or Boolean) to Visibility
        /// But instead of true=visible, it opposite
        /// </summary>
        /// <param name="input">Any boolean input</param>
        /// <returns></returns>
        public static Visibility BoolToVisibilityInvert(bool input)
        {
            if (input)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoBulletOptionsShow(bool bullet, bool bold, bool strikethrough, bool underline, bool italic)
        {
            if (!bullet && !bold && !strikethrough && !underline && !italic)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoAlignButtonShow(bool left, bool center, bool right, bool justify)
        {
            if (!left && !center && !right && !justify)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoFormatsButtonShow(bool font, bool color, bool emoji)
        {
            if (!font && !color && !emoji)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoSizeButtonShow(bool up, bool down)
        {
            if (!up && !down)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static SolidColorBrush FromColorToBrush(Color input)
        {
            return new SolidColorBrush(input);
        }

        public static string SwitchBetweenOverlayIcon(bool input)
        {
            if (input)
            {
                return "\uEE47";
            }
            else
            {
                return "\uEE49";
            }
        }

        /// <summary>
        /// Use to check if input item is null or not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsItemNull(object item)
        {
            return item is null;
        }

        /// <summary>
        /// Use in XAML binding, if item is null hide that UI, if item is not null show the UI
        /// </summary>
        /// <param name="input">anything</param>
        /// <returns></returns>
        public static Visibility ShowIfItemIsNotNull(object input) => IsItemNull(input) ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility CanIShowStatusBar(bool classicMode, bool focus, bool over, bool showStatusBar)
        {
            //Is it classic mode?
            if (classicMode)
            {
                //If it on either mode, is it allow to show status bar?
                if (showStatusBar)
                {
                    return Visibility.Visible;
                }
            }
            else if (!focus && !over && !classicMode)
            {
                //Is not in any mode (focus, overlay, classic)
                if (showStatusBar)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Use to compare number and return boolean
        /// </summary>
        /// <param name="input">Number input</param>
        /// <param name="compare">Compare type</param>
        /// <param name="target">Number to compare to</param>
        /// <returns></returns>
        public static bool CompareNumber(int input, string compare, int target)
        {
            switch (compare)
            {
                case IntCompare.NotEqual:
                    return input != target;
                case IntCompare.LessOrEqual:
                    return input <= target;
                case IntCompare.MoreOrEqual:
                    return input >= target;
                case IntCompare.Less:
                    return input < target;
                case IntCompare.More:
                    return input > target;
                case IntCompare.Equal:
                default:
                    return input == target;
            }
        }

        public static Visibility ShowAfterCompareNumber(int number, string compareType, int target)
        {
            return CompareNumber(number, compareType, target) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Brush SelectionBetweenBrush(bool determiner, Brush a, Brush b) => determiner ? a : b;

        public static FontFamilyItem SelectionFromString(string name, IList<FontFamilyItem> fonts)
        {
            if (fonts is null)
                return new FontFamilyItem(App.QSetting.DefaultFont);
            return fonts.FirstOrDefault(i => i.Name == name);
        }

        public static Color GetColorFromHex(string hex)
        {
            if (!hex.StartsWith("#"))
            {
                if (hex == "Default")
                    return new UISettings().GetColorValue(UIColorType.Background);
                else
                    return (Color)XamlBindingHelper.ConvertValue(typeof(Color), hex);
            }
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        public static string GetHexFromColor(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }

    public static class IntCompare
    {
        public const string Equal = "=";
        public const string NotEqual = "!=";
        public const string Less = "<";
        public const string More = ">";
        public const string LessOrEqual = "<=";
        public const string MoreOrEqual = ">=";
    }
}