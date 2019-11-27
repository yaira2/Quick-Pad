using System;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvvm.ViewModels
{
    public class FindAndReplaceViewModel : ViewModel, IFindAndReplaceView
    {
        private DocumentViewModel _documentViewModel;
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

        public string FindNext(DocumentViewModel viewModel) => SearchNext?.Invoke(viewModel);
        public string FindPrevious(DocumentViewModel viewModel) => SearchPrevious?.Invoke(viewModel);
        public string ReplaceNext(DocumentViewModel viewModel) => SearchReplaceNext?.Invoke(viewModel);
        public string ReplaceAll(DocumentViewModel viewModel) => SearchReplaceAll?.Invoke(viewModel);

        public event Func<DocumentViewModel, string> SearchNext;
        public event Func<DocumentViewModel, string> SearchPrevious;
        public event Func<DocumentViewModel, string> SearchReplaceNext;
        public event Func<DocumentViewModel, string> SearchReplaceAll;
    }
}