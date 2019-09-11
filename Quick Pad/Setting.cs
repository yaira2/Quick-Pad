using Microsoft.AppCenter.Analytics;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        /// <summary>
        /// this is to check the default file extension choosen in the save file dialog
        /// </summary>
        [DefaultValue(".rtf")]
        public string DefaultFileType
        {
            get => Get<string>();
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
            set => Set(value);
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

        [DefaultValue(0)]
        public int NewUser
        {
            get => Get<int>();
            set => Set(value);
        }
        #endregion

        #region Toolbar setting
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
            set => Set(value);
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
        [DefaultValue(0)]
        public int TimesUsingFocusMode 
        { //Use to track focus mode, if less than 2 times it will show tip, else it won't
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(2)]
        public int GlobalButtonCorner
        {
            get => Get<int>();
            set => Set(value);
        }

        [DefaultValue(4)]
        public int GlobalDialogCorner
        {
            get => Get<int>();
            set => Set(value);
        }

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
            appConfig += $"#{Windows.ApplicationModel.Package.Current.DisplayName}\r\n";
            appConfig += $"#{((Window.Current.Content as Frame).Content as MainPage).VersionNumberText}\r\n";
            foreach (var set in localSettings.Values.ToList().OrderBy(i => i.Key))
            {
                appConfig += $"[{set.Value.GetType().Name}]{set.Key}={set.Value}\r\n";
            }
            return appConfig;
        }

        public void ImportSetting(string settings, bool replace)
        {
            string[] lines = settings.Split("'\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith('#'))
                    continue;
                var infos = line.Split(new char[] { '[', ']', '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (localSettings.Values.ContainsKey(infos[0]))
                {
                    localSettings.Values.Remove(infos[0]);
                }
                switch (infos[0])
                {
                    case "String":
                        if (!localSettings.Values.ContainsKey(infos[1]))
                        {
                            localSettings.Values.Add(infos[1], infos[2]);
                        }
                        break;
                    case "Boolean":
                        if (!localSettings.Values.ContainsKey(infos[1]))
                        {
                            localSettings.Values.Add(infos[1], infos[2] == "True");
                        }
                        break;
                    case "Int32":
                        if (!localSettings.Values.ContainsKey(infos[1]))
                        {
                            localSettings.Values.Add(infos[1], int.Parse(infos[2]));
                        }
                        break;
                }
            }
        }

        #endregion

        #region Events when setting change
        public autoSaveChange afterAutoSaveChanged { get; set; }
        public themeChange afterThemeChanged { get; set; }
        public fontsizeChange afterFontSizeChanged { get; set; }
        #endregion
    }

    public delegate void autoSaveChange(bool to);
    public delegate void themeChange(ElementTheme to);
    public delegate void fontsizeChange(int to);

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
        /// Use to convert on XAML & x:Bind from ElementTheme and string to boolean
        /// If the ElementTheme match string it will return true, otherwise false
        /// </summary>
        /// <param name="input">ThemeElement setting</param>
        /// <param name="expect">Expected value in string</param>
        public static bool ThemeToBool(ElementTheme input, string expect)
        {
            var convert = (ElementTheme)Enum.Parse(typeof(ElementTheme), expect);
            return Equals(input, convert);
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

        /// <summary>
        /// A very specific converter use on XAML and x:Bind that only use on align button separator
        /// By tying all visibility setting into one place and when all 4 is false it will return Visible
        /// </summary>
        /// <param name="left">Left align button</param>
        /// <param name="center">Center align button</param>
        /// <param name="right">Right align button</param>
        /// <param name="justify">Justify align button</param>
        /// <returns></returns>
        public static Visibility HideIfNoAlignButtonShow(bool left, bool center, bool right, bool justify)
        {
            if (!left && !center && !right && !justify)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static  Visibility HideIfNoSizeButtonShow(bool up, bool down)
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

        public static bool IfStringMatch(string input, string compare)
        {
            return Equals(input, compare);
        }

        public static Visibility IfStringMatchShow(string input, string compare)
        {
            if (IfStringMatch(input, compare))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public static Visibility IfStringMatchHide(string input, string compare)
        {
            if (IfStringMatch(input, compare))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public static string FromColorToHex(Color input)
        {
            return $"#{input.A.ToString("16")}{input.R.ToString("16")}{input.G.ToString("16")}{input.B.ToString("16")}";
        }

        public static Color FromHexToColor(string input)
        {
            input = input.Substring(1);
            byte a = (byte)Convert.ToUInt32(input.Substring(0, 2), 16);
            byte r = (byte)Convert.ToUInt32(input.Substring(2, 2), 16);
            byte g = (byte)Convert.ToUInt32(input.Substring(4, 2), 16);
            byte b = (byte)Convert.ToUInt32(input.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
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
        /// Use in XAML binding, if item is null show that UI, if item is not null hide the UI
        /// </summary>
        /// <param name="input">anything</param>
        /// <returns></returns>
        public static Visibility ShowIfItemIsNull(object input) => IsItemNull(input) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Use in XAML binding, if item is null hide that UI, if item is not null show the UI
        /// </summary>
        /// <param name="input">anything</param>
        /// <returns></returns>
        public static Visibility ShowIfItemIsNotNull(object input) => IsItemNull(input) ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility CanIShowStatusBar(bool classicMode, bool showStatusBar)
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

        public static FontFamily FontNameToFontFamily(string name)
        {
            return new FontFamily(name);
        }

        public static CornerRadius IntToCorner(int corner)
        {
            return new CornerRadius(corner);
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