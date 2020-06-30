using System;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvvm.ViewModels
{
    public class FindAndReplaceViewModel<TStorageFile, TStream> : ViewModel<TStorageFile, TStream>, IFindAndReplaceView<TStorageFile, TStream>
        where TStream : class
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

        public SettingsViewModel<TStorageFile, TStream> Settings { get; }

        public FindAndReplaceViewModel(ILogger<IFindAndReplaceView<TStorageFile, TStream>> logger
            , SettingsViewModel<TStorageFile, TStream> settings
            , IApplication<TStorageFile, TStream> app) : base(logger, app)
        {
            Settings = settings;
        }

        public (string text, string match, int start, int length) FindNext(DocumentViewModel<TStorageFile, TStream> viewModel) => SearchNext?.Invoke(Settings, viewModel.Text.Replace(Environment.NewLine,  viewModel.IsRtf ? "\r" : Environment.NewLine), GetOffset(viewModel), viewModel) ?? default;
        public (string text, string match, int start, int length) FindPrevious(DocumentViewModel<TStorageFile, TStream> viewModel) => SearchPrevious?.Invoke(Settings, viewModel.Text.Replace(Environment.NewLine, viewModel.IsRtf ? "\r" : Environment.NewLine), GetOffset(viewModel, SearchDirection.Backwards), viewModel) ?? default;
        public (string text, string match, int start, int length) ReplaceNext(DocumentViewModel<TStorageFile, TStream> viewModel) => SearchReplaceNext?.Invoke(Settings, viewModel.Text.Replace(Environment.NewLine, viewModel.IsRtf ? "\r" : Environment.NewLine), GetOffset(viewModel), viewModel) ?? default;
        public (string text, string match, int start, int length)[] ReplaceAll(DocumentViewModel<TStorageFile, TStream> viewModel) => SearchReplaceAll?.Invoke(Settings, viewModel.Text.Replace(Environment.NewLine, viewModel.IsRtf ? "\r" : Environment.NewLine), viewModel);

        public event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchNext;
        public event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchPrevious;
        public event Func<SettingsViewModel<TStorageFile, TStream>, string, int, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)> SearchReplaceNext;
        public event Func<SettingsViewModel<TStorageFile, TStream>, string, DocumentViewModel<TStorageFile, TStream>, (string text, string match, int start, int length)[]> SearchReplaceAll;

        private int GetOffset(DocumentViewModel<TStorageFile, TStream> viewModel, SearchDirection direction = SearchDirection.Forwards) =>
            (viewModel.SelectedText.Length > 0)
                ? viewModel.CurrentPosition.start + (int)direction
                : viewModel.CurrentPosition.start;
        
        private enum SearchDirection { Forwards = 1, Backwards = 0 }

        public void InvokeClosed()
        {
            Closed?.Invoke();
        }

        public event Action Closed;
    }
}