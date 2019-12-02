using System;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Views
{
    public interface IFindAndReplaceView : IView
    {
        bool UseRegex { get; set; }
        bool MatchCase { get; set; }
        
        string SearchPattern { get; set; }
        string ReplacePattern { get; set; }

        Task<(string text, string match, int start, int length)> FindNext(DocumentViewModel viewModel);
        Task<(string text, string match, int start, int length)> FindPrevious(DocumentViewModel viewModel);

        Task<(string text, string match, int start, int length)> ReplaceNext(DocumentViewModel viewModel);
        Task<(string text, string match, int start, int length)[]> ReplaceAll(DocumentViewModel viewModel);

        event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchNext;
        event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchPrevious;

        event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchReplaceNext;
        event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)[]>> SearchReplaceAll;
    }
}