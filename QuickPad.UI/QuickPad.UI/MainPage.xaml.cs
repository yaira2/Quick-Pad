using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using QuickPad.MVC;
using QuickPad.MVVM;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickPad.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDocumentView
    {
        public MainPage()
        {
            App.Controller.AddView(this);
            Initialize?.Invoke(this);

            this.InitializeComponent();
            ViewModel.Document = RichEditBox.Document;

            DataContext = ViewModel;

            ViewModel.InitNewDocument();
        }

        public DocumentViewModel ViewModel { get; set; }
        public event Action<IDocumentView> Initialize;
    }
}
