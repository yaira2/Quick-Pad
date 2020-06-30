using Windows.ApplicationModel.Resources;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.UI.Helpers
{
    public class WindowsDocumentViewModelStrings : IDocumentViewModelStrings
    {
        public string RichTextDescription { get; }
        public string TextDescription { get; }
        public string Untitled { get; }

        public WindowsDocumentViewModelStrings(ResourceLoader resourceLoader)
        {
            RichTextDescription = resourceLoader.GetString("RichTextDescription");
            TextDescription = resourceLoader.GetString("TextDescription");
            Untitled = resourceLoader.GetString("Untitled");
        }
    }
}