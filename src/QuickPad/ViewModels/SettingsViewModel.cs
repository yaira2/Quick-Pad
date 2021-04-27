using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace QuickPad.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// Gets or sets a value indicating the default font.
        /// </summary>
        public string DefaultFont
        {
            get => Get("Consolas");
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets a value indicating the default font size.
        /// </summary>
        public Int32 DefaultFontSize
        {
            get => Get(12);
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether spell check is on.
        /// </summary>
        public bool IsSpellCheckOn
        {
            get => Get(false);
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto save is on.
        /// </summary>
        public bool IsAutoSaveOn
        {
            get => Get(true);
            set => Set(value);
        }

        #region ReadAndSaveSettings
        public bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = null)
        {
            propertyName = propertyName != null && propertyName.StartsWith("set_", StringComparison.InvariantCultureIgnoreCase)
                ? propertyName.Substring(4)
                : propertyName;

            TValue originalValue = default;

            if (localSettings.Values.ContainsKey(propertyName))
            {
                originalValue = Get(originalValue, propertyName);

                localSettings.Values[propertyName] = value;
                if (!base.SetProperty(ref originalValue, value, propertyName))
                {
                    return false;
                }
            }
            else
            {
                localSettings.Values[propertyName] = value;
            }

            return true;
        }

        public TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = null)
        {
            var name = propertyName ??
                       throw new ArgumentNullException(nameof(propertyName), "Cannot store property of unnamed.");

            name = name.StartsWith("get_", StringComparison.InvariantCultureIgnoreCase)
                ? propertyName.Substring(4)
                : propertyName;

            if (localSettings.Values.ContainsKey(name))
            {
                var value = localSettings.Values[name];

                if (!(value is TValue tValue))
                {
                    if (value is IConvertible)
                    {
                        tValue = (TValue)Convert.ChangeType(value, typeof(TValue));
                    }
                    else
                    {
                        var valueType = value.GetType();
                        var tryParse = typeof(TValue).GetMethod("TryParse", BindingFlags.Instance | BindingFlags.Public);

                        if (tryParse == null)
                        {
                            return default;
                        }

                        var stringValue = value.ToString();
                        tValue = default;

                        var tryParseDelegate =
                            (TryParseDelegate<TValue>)Delegate.CreateDelegate(valueType, tryParse, false);

                        tValue = (tryParseDelegate?.Invoke(stringValue, out tValue) ?? false) ? tValue : default;
                    }

                    Set(tValue, propertyName); // Put the corrected value in settings.
                    return tValue;
                }
                return tValue;
            }

            localSettings.Values[propertyName] = defaultValue;

            return defaultValue;
        }

        private delegate bool TryParseDelegate<TValue>(string inValue, out TValue parsedValue);

        #endregion ReadAndSaveSettings
    }
}
