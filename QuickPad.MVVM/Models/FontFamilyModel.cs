using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickPad.MVVM.Models
{
    public class FontFamilyModel : INotifyPropertyChanged, IComparable<FontFamilyModel>
    {
        private const int PREVIEW_MAX_LENGTH = 16;
        private static string _previewText;

        public FontFamilyModel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string PreviewText => _previewText ?? Name;

        public int CompareTo(FontFamilyModel other) => 
            string.Compare(Name, other.Name, StringComparison.Ordinal);

        public event PropertyChangedEventHandler PropertyChanged;

        public static void ChangeGlobalPreview(string previewText)
        {
            previewText = previewText?.Trim();
            if (string.IsNullOrWhiteSpace(previewText))
            {
                _previewText = null;
            }
            else
            {
                _previewText = previewText.Length > PREVIEW_MAX_LENGTH 
                    ? $"{previewText.Substring(0, PREVIEW_MAX_LENGTH)}..." 
                    : previewText;
            }
        }

        public void UpdateLocalPreview() => UpdateProperty(nameof(PreviewText));

        public override string ToString() => Name;

        private void UpdateProperty([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}