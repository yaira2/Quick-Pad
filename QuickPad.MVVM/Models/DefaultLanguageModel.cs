using System.Globalization;

namespace QuickPad.MVVM
{
    public class DefaultLanguageModel
    {
        public string Name;
        public string ID;

        public DefaultLanguageModel()
        {
            Name = "";
            ID = "";
        }

        public DefaultLanguageModel(string id)
        {
            var info = new CultureInfo(id);
            ID = info.Name;
            Name = info.DisplayName;
        }
    }
}
