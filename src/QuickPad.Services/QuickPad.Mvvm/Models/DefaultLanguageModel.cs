using System.Globalization;

namespace QuickPad.Mvvm.Models
{
    public class DefaultLanguageModel
    {
        public string ID;
        public string Name;

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

        public override string ToString()
        {
            return Name;
        }
    }
}