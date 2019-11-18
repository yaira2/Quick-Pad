using Windows.UI.Xaml;
using QuickPad.MVVM.ViewModels;
using QuickPad.UI.Common;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class CommandBar
    {
        public VisualThemeSelector VTSelector { get; } = VisualThemeSelector.Default;

        public DocumentViewModel ViewModel
        {
            get => DataContext as DocumentViewModel;
            set
            {
                if (value == null || DataContext == value) return;
                DataContext = value;
            }
        }

        public static DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DocumentViewModel), typeof(CommandBar), null);

        public CommandBar()
        {
            this.InitializeComponent();
        }

    }
}
