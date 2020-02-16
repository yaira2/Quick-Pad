using Windows.UI.Text;
using QuickPad.Mvvm.Models;

namespace QuickPad.UI.Helpers
{
    public static class TextOptionsConverters
    {
        public static TextGetOptions ToUwp(this QuickPadTextGetOptions options)
        {
            return TextGetOptions.TryParse<TextGetOptions>(options.ToString(), out var result) ? result : default;
        }

        public static TextSetOptions ToUwp(this QuickPadTextSetOptions options)
        {
            return TextSetOptions.TryParse<TextSetOptions>(options.ToString(), out var result) ? result : default;
        }

        public static QuickPadTextGetOptions ToUwp(this TextGetOptions options)
        {
            return QuickPadTextGetOptions.TryParse<QuickPadTextGetOptions>(options.ToString(), out var result) ? result : default;
        }

        public static QuickPadTextSetOptions ToUwp(this TextSetOptions options)
        {
            return QuickPadTextSetOptions.TryParse<QuickPadTextSetOptions>(options.ToString(), out var result) ? result : default;
        }
    }
}