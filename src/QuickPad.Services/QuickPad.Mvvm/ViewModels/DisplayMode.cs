using Windows.ApplicationModel.Resources;

namespace QuickPad.Mvvm.ViewModels
{
    public class DisplayMode
    {
        private ResourceLoader _resourceLoader;
        private string _text = null;
            
        public string Uid { get; }
        public string Text 
        { 
            get 
            {
                if (_text == null)
                {
                    if (_resourceLoader == null) _resourceLoader = ResourceLoader.GetForCurrentView();

                    _text = _resourceLoader.GetString($"{Uid}");
                }

                return _text;
            } 
        }

        public DisplayMode(string uid)
        {
            Uid = uid;
        }
    }
}
