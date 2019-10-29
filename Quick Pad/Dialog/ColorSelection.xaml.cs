using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        }

        FontColorItem _fci;
        public FontColorItem FontColorSelection
        {
            get => _fci;
            set
            {
                Set(ref _fci, value);
                if (value.TechnicalName.StartsWith("#"))
                    SelectedColorType = ColorSelectiomType.Custom;
                else if (value.TechnicalName == "Default")
                    SelectedColorType = ColorSelectiomType.Default;
                else
                    SelectedColorType = ColorSelectiomType.Standard;
            }
        }

        ColorSelectiomType _cstype;
        public ColorSelectiomType SelectedColorType
        {
            get => _cstype;
            set
            {
                Set(ref _cstype, value);
                NotifyPropertyChanged(nameof(IsDefaultTab));
                NotifyPropertyChanged(nameof(IsStandardTab));
                NotifyPropertyChanged(nameof(IsCustomTab));
            }
        }

        public bool IsDefaultTab => SelectedColorType == ColorSelectiomType.Default;
        public bool IsStandardTab => SelectedColorType == ColorSelectiomType.Standard;
        public bool IsCustomTab => SelectedColorType == ColorSelectiomType.Custom;

        public void SetToDefaultColor() => FontColorSelection = new DefaultFontColorItem();

        public void SetToStandardColor() =>
            //Only switch to that tab
            SelectedColorType = ColorSelectiomType.Standard;

        public void SetToCustomColor() =>
            SelectedColorType = ColorSelectiomType.Custom;
    }

    public enum ColorSelectiomType
    {
        Default,
        Standard,
        Custom
    }
}
