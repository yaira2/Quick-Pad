using System;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvvm.Views
{
    public interface IFindAndReplaceView : IView
    {
        bool UseRegex { get; set; }
        bool MatchCase { get; set; }
        
        string SearchPattern { get; set; }
        string ReplacePattern { get; set; }

        (string text, string match, int start, int length) FindNext(DocumentViewModel viewModel);
        (string text, string match, int start, int length) FindPrevious(DocumentViewModel viewModel);

        (string text, string match, int start, int length) ReplaceNext(DocumentViewModel viewModel);
        (string text, string match, int start, int length)[] ReplaceAll(DocumentViewModel viewModel);

        event Func<SettingsViewModel, string, int, DocumentViewModel, (string text, string match, int start, int length)> SearchNext;
        event Func<SettingsViewModel, string, int, DocumentViewModel, (string text, string match, int start, int length)> SearchPrevious;

        event Func<SettingsViewModel, string, int, DocumentViewModel, (string text, string match, int start, int length)> SearchReplaceNext;
        event Func<SettingsViewModel, string, DocumentViewModel, (string text, string match, int start, int length)[]> SearchReplaceAll;
    }
}