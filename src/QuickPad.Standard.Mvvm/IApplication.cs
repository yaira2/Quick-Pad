using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm
{
    public interface IApplication<TStorageFile, TStream>
        where TStream : class
    {
        DocumentViewModel<TStorageFile, TStream> CurrentViewModel { get; }
        SettingsViewModel<TStorageFile, TStream> SettingsViewModel { get; }

        Task<TResult> AwaitableRunAsync<TResult>(Func<TResult> action);
        void TryEnqueue(Action action);
        void DoWhenIdle(Action action);
    }
}