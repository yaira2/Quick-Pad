using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands
{
    public interface IQuickPadCommands<TStorageFile, TStream>
        where TStream : class
    {
        SimpleCommandBase<DocumentViewModel<TStorageFile, TStream>> SaveCommandBase { get; }
        SimpleCommandBase<DocumentViewModel<TStorageFile, TStream>> NewDocumentCommandBase { get; }
        SimpleCommandBase<DocumentViewModel<TStorageFile, TStream>> LoadCommandBase { get; }
        SimpleCommandBase<DocumentViewModel<TStorageFile, TStream>> SaveAsCommandBase { get; }
        SimpleCommandBase<DocumentViewModel<TStorageFile, TStream>> ExitCommandBase { get; }

        void NotifyAll(DocumentViewModel<TStorageFile, TStream> documentViewModel,
            SettingsViewModel<TStorageFile, TStream> settings);
    }
}