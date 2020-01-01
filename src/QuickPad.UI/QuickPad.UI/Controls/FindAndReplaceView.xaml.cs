using Windows.UI.Xaml.Controls;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models.Theme;
using QuickPad.Mvvm.ViewModels;
using QuickPad.Mvvm.Views;
using QuickPad.UI.Common.Theme;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class FindAndReplaceView : UserControl
    {
        private DocumentViewModel _documentViewModel;
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

        public SettingsViewModel Settings => App.Settings;

        public QuickPadCommands Commands => App.Commands;

        public IFindAndReplaceView ViewModel => DocumentViewModel?.FindAndReplaceViewModel;
        
        public DocumentViewModel DocumentViewModel
        {
            get => _documentViewModel;
            set
            {
                if (_documentViewModel == value || value == null) return;

                _documentViewModel = value;

                DataContext = _documentViewModel.FindAndReplaceViewModel;

                App.Controller.AddView(ViewModel);
            }
        }

        public FindAndReplaceView()
        {
            this.InitializeComponent();

            if (ViewModel != null)
            {
                DataContext = ViewModel;
            }
        }

    }
}
