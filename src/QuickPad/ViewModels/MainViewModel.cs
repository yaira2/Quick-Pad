using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml;

namespace QuickPad.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public void NewFileCommand()
        {
            // Setup new document
            PrepareNewDocument(null);
        }

        public void PrepareNewDocument(string FileName)
        {
            Font = App.SettingsViewModel.DefaultFont;
            FontSize = App.SettingsViewModel.DefaultFontSize;

            // Set document name
            if (FileName == null)
            {
                DocumentName = "Untitled";
            }
            else
            {
                DocumentName = FileName;
            }
        }

        private string documentName;

        /// <summary>
        /// Gets or sets a value indicating the current document name.
        /// </summary>
        public string DocumentName
        {
            get => documentName;
            set => SetProperty(ref documentName, value);
        }

        private string font;

        /// <summary>
        /// Gets or sets a value indicating the current font.
        /// </summary>
        public string Font
        {
            get => font;
            set => SetProperty(ref font, value);
        }

        private Int32 fontSize;

        /// <summary>
        /// Gets or sets a value indicating the current font size.
        /// </summary>
        public Int32 FontSize
        {
            get => fontSize;
            set => SetProperty(ref fontSize, value);
        }
    }
}