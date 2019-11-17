using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickPad.MVVM
{
    public class FontFamilyModel : INotifyPropertyChanged, IComparable<FontFamilyModel>
    {
        private const int previewMaxLenght = 16;
        private static string _previewText;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get;
            private set;
        }
        public string PreviewText
        {
            get => _previewText ?? Name;
        }

        public FontFamilyModel(string name)
        {
            Name = name;
        }

        public static void ChangeGlobalPreview(string previewText)
        {
            if (previewText != null)
            {
                previewText = previewText.Trim();
            }
            if (string.IsNullOrWhiteSpace(previewText))
            {
                _previewText = null;
            }
            else
            {
                previewText = previewText.Trim();
                if (previewText.Length > previewMaxLenght)
                {
                    _previewText = $"{previewText.Substring(0, previewMaxLenght)}...";
                }
                else
                {
                    _previewText = previewText;
                }
            }
        }
        public void UpdateLocalPreview()
        {
            UpdateProperty(nameof(PreviewText));
        }

        public int CompareTo(FontFamilyModel other)
        {
            return this.Name.CompareTo(other.Name);
        }
        public override string ToString()
        {
            return Name;
        }
        private void UpdateProperty([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
