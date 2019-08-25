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

        #region Dialog Dependencies
        bool _replace;
        public bool ShowReplace
        {
            get => _replace;
            set => Set(ref _replace, value);
        }

        bool _literal;
        public bool MatchCase
        {
            get => _literal;
            set => Set(ref _literal, value);
        }

        bool _wrap;
        public bool WrapAround
        {
            get => _wrap;
            set => Set(ref _wrap, value);
        }

        bool _ff;
        public bool FindForward
        {
            get => _ff;
            set => Set(ref _ff, value);
        }
        #endregion

        #region Input Dependencies
        public string TextToFind
        {
            get => (string)GetValue(TextToFindProperty);
            set => SetValue(TextToFindProperty, value);
        }
        public static readonly DependencyProperty TextToFindProperty = DependencyProperty.Register(
            nameof(TextToFindProperty),
            typeof(string),
            typeof(UserControl),
            new PropertyMetadata("", null));

        public string TextToReplace
        {
            get => (string)GetValue(TextToReplaceProperty);
            set => SetValue(TextToReplaceProperty, value);
        }
        public static readonly DependencyProperty TextToReplaceProperty = DependencyProperty.Register(
            nameof(TextToReplaceProperty),
            typeof(string),
            typeof(UserControl),
            new PropertyMetadata("", null));

        #endregion

        #region Events
        public CloseDialog onClosed { get; set; }
        public RequestFind onRequestFinding { get; set; }
        public RequestReplace onRequestReplacing { get; set; }
        #endregion

        #region Buttons
        private void RequestCloseDialog() => onClosed?.Invoke();

        public void RequestFindNext()
        {
            FindForward = true;
            onRequestFinding?.Invoke(TextToFind, FindForward, MatchCase, WrapAround);
        }

        public void RequestFindPrevious()
        {
            FindForward = false;
            onRequestFinding?.Invoke(TextToFind, FindForward, MatchCase, WrapAround);
        }

        public void SendRequestFind() => onRequestFinding?.Invoke(TextToFind, FindForward, MatchCase, WrapAround);

        public void SendRequestReplace() => onRequestReplacing?.Invoke(TextToFind, TextToReplace, FindForward, MatchCase, WrapAround, false);

        public void SendRequestReplaceAll() => onRequestReplacing?.Invoke(TextToFind, TextToReplace, FindForward, MatchCase, WrapAround, true);
        #endregion
    }

    public delegate void CloseDialog();
    public delegate void RequestFind(string find, bool direction, bool match, bool wrap);
    public delegate void RequestReplace(string find, string replace, bool direction, bool match, bool wrap, bool all);
}
