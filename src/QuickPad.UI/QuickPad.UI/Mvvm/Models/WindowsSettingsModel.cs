using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Theme;

namespace QuickPad.UI.Helpers
{
    public class WindowsSettingsModel : SettingsModel<StorageFile, IRandomAccessStream>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDataContainer _roamingSettings;
        private string _defaultColor;

        public WindowsSettingsModel(ILogger<SettingsViewModel<StorageFile, IRandomAccessStream>> logger
            , IApplication<StorageFile, IRandomAccessStream> app
            , IServiceProvider serviceProvider) 
            : base(logger, app)
        {
            _serviceProvider = serviceProvider;
            _roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        public void ResetSettings()
        {
            _roamingSettings.Values.Clear();
        }

        public string DefaultTextForegroundColorString
        {
            get => Get(Colors.White.ToHex());
            set => Set(value);
        }

        public string DefaultTextForegroundBrushString
        {
            get
            {
                _defaultColor ??= _serviceProvider.GetService<IVisualThemeSelector>().CurrentItem
                    .DefaultTextForegroundColor.ToHex();

                return Get(_defaultColor);
            }
            set => Set(value);
        }

        public string DefaultLanguageString
        {
            get => Get("en-US");
            set => Set(value);
        }

        public double BackgroundTintOpacity
        {
            get => Get(0.75);
            set => Set(value);
        }

        public string FlowDirection
        {
            get => Get(nameof(Windows.UI.Xaml.FlowDirection.LeftToRight));
            set => Set(value);
        }

        public override bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = null)
        {
            propertyName = propertyName != null && propertyName.StartsWith("set_", StringComparison.InvariantCultureIgnoreCase)
                ? propertyName.Substring(4)
                : propertyName;

            TValue originalValue = default;
            
            if(_roamingSettings.Values.ContainsKey(propertyName))
            {
                originalValue = Get(originalValue, propertyName);

                if (!base.Set(ref originalValue, value, propertyName)) return false;
            }

            _roamingSettings.Values[propertyName] = value;

            return true;
        }

        public override TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = null)
        {
            var name = propertyName ??
                       throw new ArgumentNullException(nameof(propertyName), "Cannot store property of unnamed.");

            name = name.StartsWith("get_", StringComparison.InvariantCultureIgnoreCase)
                ? propertyName.Substring(4)
                : propertyName;

            Logger.LogDebug($"WindowsSettingsModel::Get<{typeof(TValue).Name}>({defaultValue}, {name});");

            if (_roamingSettings.Values.ContainsKey(name))
            {
                var value = (TValue)_roamingSettings.Values[name];

                Logger.LogDebug(
                    $"WindowsSettingsModel::Get<{typeof(TValue).Name}>({defaultValue}, {name}) -> {value};");

                return value;
            }

            Logger.LogDebug(
                $"WindowsSettingsModel::Get<{typeof(TValue).Name}>({defaultValue}, {name}) -> {defaultValue} (default value);");

            return defaultValue;
        }
    }
}