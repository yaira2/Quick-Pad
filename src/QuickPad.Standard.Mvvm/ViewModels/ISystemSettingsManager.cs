using System.Runtime.CompilerServices;

namespace QuickPad.Mvvm.ViewModels
{
    public interface ISystemSettingsManager<TStorageFile, TStream>
        where TStream : class
    {
        bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = null);
        TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = null);
        bool Set<TValue>(ref TValue original, TValue value, [CallerMemberName] string propertyName = null);
        DocumentViewModel<TStorageFile, TStream> CurrentViewModel { get; }
        string GetTranslation(string resourceName);
    }
}