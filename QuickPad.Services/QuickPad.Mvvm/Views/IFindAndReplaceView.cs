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

        string FindNext(DocumentViewModel viewModel);
        string FindPrevious(DocumentViewModel viewModel);

        string ReplaceNext(DocumentViewModel viewModel);
        string ReplaceAll(DocumentViewModel viewModel);

        event Func<DocumentViewModel, string> SearchNext;
        event Func<DocumentViewModel, string> SearchPrevious;

        event Func<DocumentViewModel, string> SearchReplaceNext;
        event Func<DocumentViewModel, string> SearchReplaceAll;

    }
}