using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvvm.ViewModels
{
    public class FindAndReplaceViewModel : ViewModel, IFindAndReplaceView
    {
        private bool _useRegex;
        private bool _matchCase;
        private string _searchPattern;
        private string _replacePattern;

        public bool UseRegex
        {
            get => _useRegex;
            set => Set(ref _useRegex, value);
        }

        public bool MatchCase
        {
            get => _matchCase;
            set => Set(ref _matchCase, value);
        }

        public string SearchPattern
        {
            get => _searchPattern;
            set => Set(ref _searchPattern, value);
        }

        public string ReplacePattern
        {
            get => _replacePattern;
            set => Set(ref _replacePattern, value);
        }

        public SettingsViewModel Settings { get; }

        public FindAndReplaceViewModel(ILogger<IFindAndReplaceView> logger
            , SettingsViewModel settings) : base(logger)
        {
            Settings = settings;
        }

        public Task<(string text, string match, int start, int length)> FindNext(DocumentViewModel viewModel) => SearchNext?.Invoke(Settings, viewModel.Text, viewModel) ?? default;
        public Task<(string text, string match, int start, int length)> FindPrevious(DocumentViewModel viewModel) => SearchPrevious?.Invoke(Settings, viewModel.Text, viewModel) ?? default;
        public Task<(string text, string match, int start, int length)> ReplaceNext(DocumentViewModel viewModel) => SearchReplaceNext?.Invoke(Settings, viewModel.Text, viewModel) ?? default;
        public Task<(string text, string match, int start, int length)[]> ReplaceAll(DocumentViewModel viewModel) => SearchReplaceAll?.Invoke(Settings, viewModel.Text, viewModel);

        public event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchNext;
        public event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchPrevious;
        public event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)>> SearchReplaceNext;
        public event Func<SettingsViewModel, string, DocumentViewModel, Task<(string text, string match, int start, int length)[]>> SearchReplaceAll;
    }
}