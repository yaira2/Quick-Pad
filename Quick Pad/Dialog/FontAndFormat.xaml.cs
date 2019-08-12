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
        /// <summary>
        /// This is a font input and also a final result
        /// </summary>
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

        string _selected;
        public string SelectedOnFilteredFont
        {
            get => _selected;
            set
            {
                if (Set(ref _selected, value))
                {
                    //Selected and update
                    //Should have put this onto input
                    if (string.IsNullOrEmpty(value))
                    {
                        FontNameSuggestionInput = value;
                    }
                }
            }
        }
        #endregion

        #region Font size and other formatting
        int _size;
        public int FontSizeSelection
        {
            get => _size;
            set => Set(ref _size, value);
        }

        bool _bold;
        public bool WantBold
        {
            get => _bold;
            set => Set(ref _bold, value);
        }

        bool _italic;
        public bool WantItalic
        {
            get => _italic;
            set => Set(ref _italic, value);
        }

        bool _under;
        public bool WantUnderline
        {
            get => _under;
            set => Set(ref _under, value);
        }

        bool _strike;
        public bool WantStrikethrough
        {
            get => _strike;
            set => Set(ref _strike, value);
        }
        #endregion

        DialogResult _final;
        public DialogResult FinalResult
        {
            get => _final;
            set => Set(ref _final, value);
        }

        private void ApplyAllSettings()
        {
            FinalResult = DialogResult.Yes;
            this.Hide();
        }

        private void DropEverything()
        {
            FinalResult = DialogResult.No;
            this.Hide();            
        }
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
