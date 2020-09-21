using QuickPad.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm
{
    public interface IApplication<TStorageFile, TStream>
        where TStream : class
    {
        DocumentViewModel<TStorageFile, TStream> CurrentViewModel { get; }
        SettingsViewModel<TStorageFile, TStream> SettingsViewModel { get; }
        IServiceProvider Services { get; }

        Task<TResult> AwaitableRunAsync<TResult>(Func<TResult> action);

        void TryEnqueue(Action action);

        void DoWhenIdle(Action action);
    }
}