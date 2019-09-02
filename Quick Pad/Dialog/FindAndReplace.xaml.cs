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
using Windows.UI.Xaml.Media.Animation;
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
            this.RegisterPropertyChangedCallback(VisibilityProperty, VisibilityChanged);
        }

        private void VisibilityChanged(DependencyObject sender, DependencyProperty dp)
        {
            if ((sender as UserControl).Visibility == Visibility.Collapsed)
            {
                ExitContentHolderStoryboard.Begin();
                
            }
            else if ((sender as UserControl).Visibility == Visibility.Visible)
            {
                EnterContentHolderStoryboard.Begin();

            }
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

        bool _ff = true;
        public bool FindForward
        {
            get => _ff;
            set => Set(ref _ff, value);
        }
        #endregion

        #region Input Dependencies
        string _tf;
        public string TextToFind
        {
            get => _tf;
            set => Set(ref _tf, value);
        }
        
        string _tr;
        public string TextToReplace
        {
            get => _tr;
            set => Set(ref _tr, value);
        }

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

        private void FindInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendRequestFind();
            }
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendRequestReplace();
            }
        }
    }

    public delegate void CloseDialog();
    public delegate void RequestFind(string find, bool direction, bool match, bool wrap);
    public delegate void RequestReplace(string find, string replace, bool direction, bool match, bool wrap, bool all);
}
