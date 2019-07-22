using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;

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
            MigrateSettingFromPreviousVersion<T>(propertyName);
            return (T)localSettings.Values[propertyName];
        }

        private void MigrateSettingFromPreviousVersion<T>(string propertyName)
        {
            if (localSettings.Values[propertyName].GetType() == typeof(T))
                return;
            object previousSetting = default;
            string conversion = "";

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
                previousSetting = (int)Enum.Parse(typeof(ElementTheme), conversion);
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
            return default(T);
        }

        public void Set<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (!localSettings.Values.ContainsKey(propertyName))
            {
                localSettings.Values.Add(propertyName, value);
                NotifyPropertyChanged(propertyName);
                return;
            }
            if (!Equals(localSettings.Values[propertyName], value))
            {
                localSettings.Values[propertyName] = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        #region Settings
        [DefaultValue((int)(AvailableModes.Default))]
        public int LaunchMode
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

        [DefaultValue(true)]
        public bool AutoSave
        {
            get => Get<bool>();
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
            set => Set((int)value);
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
        #endregion
    }

    public enum AvailableModes
    {
        Default, //Full UI with toolbar
        OnTop, //Compact overlay
        Focus //Hide toolbar
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
    }
}