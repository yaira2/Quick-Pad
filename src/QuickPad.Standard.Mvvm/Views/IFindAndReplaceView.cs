using System;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Views
{
    public interface IFindAndReplaceView<TStorageFile, TStream> : IView
        where TStream : class
    {
        bool UseRegex { get; set; }
        bool MatchCase { get; set; }
        
        string SearchPattern { get; set; }
        string ReplacePattern { get; set; }

        (string text, string match, int start, int length) FindNext(DocumentViewModel<TStorageFile, TStream> viewModel);
        (string text, string match, int start, int length) FindPrevious(DocumentViewModel<TStorageFile, TStream> viewModel);

        (string text, string match, int start, int length) ReplaceNext(DocumentViewModel<TStorageFile, TStream> viewModel);
        (string text, string match, int start, int length)[] ReplaceAll(DocumentViewModel<TStorageFile, TStream> viewModel);

        event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchNext;
        event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchPrevious;

        event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchReplaceNext;
        event Func<SettingsViewModel<TStorageFile, TStream>, string, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)[]> SearchReplaceAll;
    }
}