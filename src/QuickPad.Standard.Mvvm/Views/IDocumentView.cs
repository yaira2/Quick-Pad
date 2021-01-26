using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Views
{
    public interface IDocumentView<TStorageFile, TStream> : IView
        where TStream : class
    {
        DocumentViewModel<TStorageFile, TStream> ViewModel { get; set; }

        event Action<IDocumentView<TStorageFile, TStream>, IQuickPadCommands<TStorageFile, TStream>, IApplication<TStorageFile, TStream>> Initialize;

        event Func<DocumentViewModel<TStorageFile, TStream>, Task<bool>> ExitApplication;

        event Func<DocumentViewModel<TStorageFile, TStream>, TStorageFile, Task> LoadFromFile;

        event Action<IDocumentView<TStorageFile, TStream>> SaveToFile;

        event Action<IDocumentView<TStorageFile, TStream>, string> SaveToCache;

        event Action<IDocumentView<TStorageFile, TStream>> CreateNewDocument;

        event Action GainedFocus;
    }
}