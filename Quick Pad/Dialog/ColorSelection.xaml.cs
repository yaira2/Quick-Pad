using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.Dialog
{
    public sealed partial class ColorSelection : UserControl, INotifyPropertyChanged
    {
        public VisualThemeSelector VisualThemeSelector { get; } = VisualThemeSelector.Default;

        public Setting QSetting => App.QSetting;

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
            set => Set(ref _fci, value);
        }

        bool _custom;
        public bool ShowCustomColor
        {
            get => _custom;
            set => Set(ref _custom, value);
        }
    }
}
