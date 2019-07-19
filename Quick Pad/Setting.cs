using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;

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
        #endregion
    }

    public enum AvailableModes
    {
        Default, //Full UI with toolbar
        OnTop, //Compact overlay
        Focus //Hide toolbar
    }
}