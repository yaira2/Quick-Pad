using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.Dialog
{
    public sealed partial class ColorSelection : UserControl, INotifyPropertyChanged
    {
        public VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

        public Setting QSetting => App.QSetting;

        public ResourceLoader textResource => ResourceLoader.GetForCurrentView();


        #region Property notification
        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        
        public ColorSelection()
        {
            this.InitializeComponent();
            FontColorItems = new ObservableCollection<FontColorInfo>()
            {
                //Row1
                new FontColorInfo("Black", "Black"),
                new FontColorInfo("Gray", "DimGray"),
                new FontColorInfo("Gray", "Gray"),
                new FontColorInfo("Gray", "DarkGray"),
                new FontColorInfo("Gray", "LightGray"),
                new FontColorInfo("White", "White"),
                //Row2
                new FontColorInfo("Blue", "Blue"),
                new FontColorInfo("Blue", "MidnightBlue"),
                new FontColorInfo("Blue", "DarkBlue"),
                new FontColorInfo("Blue", "MediumBlue"),
                new FontColorInfo("Blue", "RoyalBlue"),
                new FontColorInfo("Blue", "LightBlue"),
                //Row3
                new FontColorInfo("Green", "Green"),
                new FontColorInfo("Green", "DarkGreen"),
                new FontColorInfo("Green", "ForestGreen"),
                new FontColorInfo("Green", "LimeGreen"),
                new FontColorInfo("Green", "LawnGreen"),
                new FontColorInfo("Green", "PaleGreen"),
                //Row4
                new FontColorInfo("Pink", "Pink"),
                new FontColorInfo("Pink", "MediumVioletRed"),
                new FontColorInfo("Pink", "DeepPink"),
                new FontColorInfo("Pink", "HotPink"),
                new FontColorInfo("Pink", "Magenta"),
                new FontColorInfo("Pink", "Violet"),
                //Row5
                new FontColorInfo("Yellow", "Yellow"),
                new FontColorInfo("Yellow", "DarkGoldenrod"),
                new FontColorInfo("Yellow", "Goldenrod"),
                new FontColorInfo("Yellow", "Gold"),
                new FontColorInfo("Yellow", "Khaki"),
                new FontColorInfo("Yellow", "LemonChiffon"),
                //Row6
                new FontColorInfo("Orange", "Orange"),
                new FontColorInfo("Orange", "Sienna"),
                new FontColorInfo("Orange", "Chocolate"),
                new FontColorInfo("Orange", "Salmon"),
                new FontColorInfo("Orange", "SandyBrown"),
                new FontColorInfo("Orange", "PeachPuff")
            };
            Initialize = true;
        }

        private bool Initialize = false;

        #region Properties
        ObservableCollection<FontColorInfo> _fc;
        public ObservableCollection<FontColorInfo> FontColorItems
        {
            get => _fc;
            set => Set(ref _fc, value);
        }

        public string FontColorSelection
        {
            get => (string)GetValue(FontColorSelectionProperty);
            set
            {
                SetValue(FontColorSelectionProperty, value);
                NotifyPropertyChanged(nameof(FinalSelectionDisplay));
                NotifyPropertyChanged(nameof(FinalSelectionPreview));
            }
        }

        public static readonly DependencyProperty FontColorSelectionProperty =
            DependencyProperty.Register(nameof(FontColorSelection),
                typeof(string),
                typeof(UserControl),
                new PropertyMetadata(App.QSetting.DefaultFontColor, null));

        public bool AllowDefault
        {
            get => (bool)GetValue(AllowDefaultProperty);
            set => SetValue(AllowDefaultProperty, value);
        }

        public static readonly DependencyProperty AllowDefaultProperty =
            DependencyProperty.Register(nameof(AllowDefault),
                typeof(bool),
                typeof(UserControl),
                new PropertyMetadata(false, null));
        #endregion

        #region Event
        private ColorSelectionChangedEventHandler _colorChanged;
        public event ColorSelectionChangedEventHandler ColorSelectionChanged
        {
            add => _colorChanged += value;
            remove => _colorChanged -= value;
        }

        #endregion

        #region Tabbing
        ColorSelectionType? _ctype = null;
        public ColorSelectionType CurrentColorTab
        {
            get
            {
                if (_ctype is null)
                {
                    if (App.QSetting.DefaultFontColor.StartsWith("#"))
                        _ctype = ColorSelectionType.Custom;
                    else if (App.QSetting.DefaultFontColor == "Default")
                        _ctype = ColorSelectionType.Default;
                    else
                        _ctype = ColorSelectionType.Standard;
                }
                return _ctype.Value;
            }
            set => Set(ref _ctype, value);
        }

        public bool CompareAndReturn(object item, string expect) => Equals(item, Enum.Parse(item.GetType(), expect));

        public Visibility CompareAndShowOrHide(object item, string expect) =>
            CompareAndReturn(item, expect) ? Visibility.Visible : Visibility.Collapsed;

        public void SetToDefaultColor()
        {
            CurrentColorTab = ColorSelectionType.Default;
            FontColorSelection = "Default";
        }

        public void SetToStandardColor() => CurrentColorTab = ColorSelectionType.Standard;

        public void SetToCustomColor() => CurrentColorTab = ColorSelectionType.Custom;
        #endregion
        
        public string FinalSelectionDisplay
        {
            get
            {
                if (FontColorSelection.StartsWith("#"))
                    return textResource.GetString("CustomColor");
                else if (FontColorSelection == "Default")
                    return ResourceLoader.GetForCurrentView().GetString("LaunchModeDefault/Content");
                else
                    return FontColorItems.First(i => i.TechnicalName == FontColorSelection).DisplayName;
            }            
        }

        public SolidColorBrush FinalSelectionPreview 
            => new SolidColorBrush(FontColorInfo.GetColorFromStoredSetting(FontColorSelection));

        public Color SettingToColor(string input)
        {
            return FontColorInfo.GetColorFromStoredSetting(input);
        }

        public void BindBackCustomFontColor(Color input)
        {
            if (!Initialize) return;
            if (CurrentColorTab == ColorSelectionType.Custom)
            {
                FontColorSelection = Converter.GetHexFromColor(input);
                _colorChanged?.Invoke(this, new ColorSelectionChangedEventArgs(FontColorSelection, input));
            }
        }

        public FontColorInfo SelectionBasedFromSetting(string setting)
        {
            Color fromSetting = FontColorInfo.GetColorFromStoredSetting(setting);
            return FontColorItems.FirstOrDefault(i => i.FinalColor == fromSetting);
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Initialize) return;
            if (sender is GridView g)
            {
                if (g.SelectedIndex > -1)
                {
                    if (CurrentColorTab == ColorSelectionType.Standard)
                    {
                        FontColorSelection = FontColorItems[g.SelectedIndex].TechnicalName;
                        _colorChanged?.Invoke(this, new ColorSelectionChangedEventArgs(FontColorItems[g.SelectedIndex]));
                    }
                }
            }
        }
    }

    public enum ColorSelectionType
    {
        Default,
        Standard,
        Custom
    }

    public class FontColorInfo : INotifyPropertyChanged
    {
        #region Property notification
        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        public void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        string _display;
        public string DisplayName
        {
            get => _display;
            set => Set(ref _display, value);
        }

        string _technical;
        public string TechnicalName
        {
            get => _technical;
            set => Set(ref _technical, value);
        }

        public FontColorInfo()
        {
            DisplayName = ResourceLoader.GetForCurrentView().GetString("LaunchModeDefault/Content");
            TechnicalName = "Default";
        }

        public FontColorInfo(string hex)
        {
            DisplayName = ResourceLoader.GetForCurrentView().GetString("CustomColor");
            TechnicalName = hex;            
        }

        public FontColorInfo(string name, string technical)
        {
            DisplayName = ResourceLoader.GetForCurrentView().GetString($"FontColor{name}");
            TechnicalName = technical;
        }

        public Color FinalColor => GetColorFromStoredSetting(TechnicalName);

        public static Color GetColorFromStoredSetting(string setting)
        {
            if (string.IsNullOrEmpty(setting))
                return new UISettings().GetColorValue(UIColorType.Foreground);
            if (setting.StartsWith("#"))
                return Converter.GetColorFromHex(setting);
            else if (setting == "Default")
                return new UISettings().GetColorValue(UIColorType.Foreground);
            else
                return (Color)XamlBindingHelper.ConvertValue(typeof(Color), setting);
        }
    }

    public delegate void ColorSelectionChangedEventHandler(object sender, ColorSelectionChangedEventArgs e);
    public class ColorSelectionChangedEventArgs : EventArgs
    {
        public Color SelectedColor { get; set; }
        public string BasedSelectionName { get; set; }
        public ColorSelectionChangedEventArgs(string select)
        {
            BasedSelectionName = select;
            SelectedColor = FontColorInfo.GetColorFromStoredSetting(select);
        }

        public ColorSelectionChangedEventArgs(string select, Color final)
        {
            BasedSelectionName = select;
            SelectedColor = final;
        }

        public ColorSelectionChangedEventArgs(FontColorInfo changed)
        {
            BasedSelectionName = changed.TechnicalName;
            SelectedColor = changed.FinalColor;
        }
    }

}
