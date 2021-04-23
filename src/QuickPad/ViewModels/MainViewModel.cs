using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;

namespace QuickPad.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private MainViewModel()
        {
        }

        public void NewFileCommand()
        {
            Font = App.SettingsViewModel.DefaultFont;
            FontSize = App.SettingsViewModel.DefaultFontSize;

        }

        private string font = App.SettingsViewModel.DefaultFont;

        public string Font
        {
            get
            {
                return font;
            }
            set
            {
                if (SetProperty(ref font, value))
                {
                    App.SettingsViewModel.DefaultFont = value;
                }
            }
        }

        private Int32 fontSize = App.SettingsViewModel.DefaultFontSize;

        public Int32 FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (SetProperty(ref fontSize, value))
                {
                    App.SettingsViewModel.DefaultFontSize = value;
                }
            }
        }
    }
}
