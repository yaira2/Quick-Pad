using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.Dialog
{
    public sealed partial class FindAndReplace : UserControl, INotifyPropertyChanged
    {
        #region Notification overhead, no need to write it thousands times on set { }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Set property and also alert the UI if the value is changed
        /// </summary>
        /// <param name="value">New value</param>
        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }
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

        public FindAndReplace()
        {
            this.InitializeComponent();
        }

        #region Dependencies
        public bool ShowReplace
        {
            get => (bool)GetValue(ShowReplaceProperty);
            set => SetValue(ShowReplaceProperty, value);
        }
        public static readonly DependencyProperty ShowReplaceProperty = DependencyProperty.Register(
            nameof(ShowReplaceProperty),
            typeof(bool),
            typeof(UserControl),
            new PropertyMetadata(false, null));

        public bool MatchCase
        {
            get => (bool)GetValue(MatchCaseProperty);
            set => SetValue(MatchCaseProperty, value);
        }
        public static readonly DependencyProperty MatchCaseProperty = DependencyProperty.Register(
            nameof(MatchCaseProperty),
            typeof(bool),
            typeof(UserControl),
            new PropertyMetadata(false, null));

        public bool WrapAround
        {
            get => (bool)GetValue(WrapAroundProperty);
            set => SetValue(WrapAroundProperty, value);
        }
        public static readonly DependencyProperty WrapAroundProperty = DependencyProperty.Register(
            nameof(WrapAroundProperty),
            typeof(bool),
            typeof(UserControl),
            new PropertyMetadata(false, null));

        #endregion
    }
}
