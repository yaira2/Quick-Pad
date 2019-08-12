using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickPad.Dialog
{
    public sealed partial class FontAndFormat : ContentDialog, INotifyPropertyChanged
    {
        #region Notification overhead
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Set property and also alert the UI if the value is changed
        /// </summary>
        /// <param name="value">New value</param>
        public bool Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Alert the UI there is a change in this property and need update
        /// </summary>
        /// <param name="name"></param>
        public void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        
        public FontAndFormat()
        {
            this.InitializeComponent();
            FilteredFonts = new ObservableCollection<string>(AllFonts);
        }

        public ObservableCollection<string> AllFonts
        {
            get => GetValue(AllFontsProperty) as ObservableCollection<string>;
            set => SetValue(AllFontsProperty, value);
        }
        public static readonly DependencyProperty AllFontsProperty = 
            DependencyProperty.Register(
                nameof(AllFonts), 
                typeof(ObservableCollection<string>), 
                typeof(UserControl), 
                new PropertyMetadata(null, null));

        #region Font name input
        string _name;
        public string FontNameSuggestionInput
        {
            get => _name;
            set
            {
                if (Set(ref _name, value))
                {
                    //Search matched result font
                    var searched = AllFonts.ToList().FindAll(font => font.Contains(value));
                    if (searched.Count < 1)
                    {
                        FilteredFonts = new ObservableCollection<string>(AllFonts);
                    }
                    else
                    {
                        FilteredFonts = new ObservableCollection<string>(searched);
                    }
                }
            }
        }

        ObservableCollection<string> _filtered;
        public ObservableCollection<string> FilteredFonts
        {
            get => _filtered;
            set => Set(ref _filtered, value);
        }
        #endregion
    }

    /// <summary>
    /// Due to the x:Bind limitation on DataTemplate, sending itself onto funtion isn't possible yet.
    /// This is a workaround for now
    /// </summary>
    public class FontFamilySpecificConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new FontFamily(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value as FontFamily).Source;
        }
    }
}
