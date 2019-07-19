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
            return (T)localSettings.Values[propertyName];
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

        #endregion
    }
}