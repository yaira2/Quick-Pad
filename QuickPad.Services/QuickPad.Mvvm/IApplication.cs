using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm
{
    public interface IApplication
    {
        DocumentViewModel CurrentViewModel { get; }
    }
}