using QuickPad.Mvvm.ViewModels;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace QuickPad.Mvvm.Views
{
    public interface IGoToLineView
    {
        DocumentViewModel ViewModel { get; set; }

        IAsyncOperation<ContentDialogResult> ShowAsync();
    }
}