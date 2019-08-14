using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
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
        }

        public ObservableCollection<FontFamilyItem> AllFonts
        {
            get => GetValue(AllFontsProperty) as ObservableCollection<FontFamilyItem>;
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
                    var searched = AllFonts.ToList().FindAll(font => font.Name.Contains(value));
                    if (searched.Count < 1)
                    {
                        FilteredFonts = new ObservableCollection<FontFamilyItem>(AllFonts);
                    }
                    else
                    {
                        FilteredFonts = new ObservableCollection<FontFamilyItem>(AllFonts);
                    }
                }
            }
        }

        ObservableCollection<FontFamilyItem> _filtered;
        public ObservableCollection<FontFamilyItem> FilteredFonts
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
                    if (!string.IsNullOrEmpty(value))
                    {
                        _name = value;
                        NotifyPropertyChanged(nameof(FontNameSuggestionInput));
                        var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                        formatting.Name = value;
                        PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
                    }
                }
            }
        }
        #endregion
        
        #region Font size and other formatting
        int _size = 18;
        public int FontSizeSelection
        {
            get => _size;
            set
            {
                Set(ref _size, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.Size = value;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
        }

        bool _bold;
        public bool WantBold
        {
            get => _bold;
            set
            {
                Set(ref _bold, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.Bold = value ? FormatEffect.On : FormatEffect.Off;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
        }

        bool _italic;
        public bool WantItalic
        {
            get => _italic;
            set
            {
                Set(ref _italic, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.Italic = value ? FormatEffect.On : FormatEffect.Off;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
        }

        bool _under;
        public bool WantUnderline
        {
            get => _under;
            set
            {
                Set(ref _under, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.Underline = value ? UnderlineType.Single : UnderlineType.None;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
        }

        bool _strike;
        public bool WantStrikethrough
        {
            get => _strike;
            set
            {
                Set(ref _strike, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.Strikethrough = value ? FormatEffect.On : FormatEffect.Off;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
        }

        Color _sc;
        public Color SelectedColor
        {
            get => _sc;
            set
            {
                Set(ref _sc, value);
                var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
                formatting.ForegroundColor = value;
                PreviewBox.TextDocument.SetDefaultCharacterFormat(formatting);
            }
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

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            FinalResult = DialogResult.None;
            FilteredFonts = new ObservableCollection<FontFamilyItem>(AllFonts);
            PreviewBox.Document.SetText(TextSetOptions.None, ResourceLoader.GetForCurrentView().GetString("FAF_Preview_Initial"));
            //Apply settings
            var formatting = PreviewBox.Document.GetDefaultCharacterFormat();
            formatting.Name = FontNameSuggestionInput;
            formatting.Size = FontSizeSelection;
            formatting.Bold = WantBold ? FormatEffect.On : FormatEffect.Off;
            formatting.Italic = WantItalic ? FormatEffect.On : FormatEffect.Off;
            formatting.Underline = WantUnderline ? UnderlineType.Single : UnderlineType.None;
            formatting.Strikethrough = WantStrikethrough ? FormatEffect.On : FormatEffect.Off;
            formatting.ForegroundColor = SelectedColor;
            var sel = PreviewBox.Document.Selection;

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
            if (value is null)
            {
                return new FontFamily("Segoe UI");
            }
            return new FontFamily(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value as FontFamily).Source;
        }
    }
}
