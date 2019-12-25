using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using QuickPad.Mvvm.ViewModels;


namespace QuickPad.Mvc
{
    public partial class ApplicationController
    {
        private static class FindAndReplaceManager
        {
            public static (string text, string match, int start, int length)[] ReplaceAll(
                SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                offset = -1;
                viewModel.SelectText(0, 0);

                (string text, string match, int start, int length) current = default;

                var results = new List<(string text, string match, int start, int length)>();

                // ReSharper disable once ComplexConditionExpression
                while ((current = ReplaceNext(settings, text, offset, viewModel)).start > -1)
                {
                    text = current.text;
                    results.Add(current);
                    offset = current.start + 1;
                }

                return results.ToArray();
            }

            public static (string text, string match, int start, int length) ReplaceNext(
                SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                var (txt, match, start, length) = SearchNext(settings, text, offset, viewModel);

                if (start > -1)
                {
                    viewModel.SelectText(start, length);
                    viewModel.SelectedText = viewModel.FindAndReplaceViewModel.ReplacePattern;

                    if (!viewModel.FindAndReplaceViewModel.UseRegex)
                    {
                        var sb = new StringBuilder(txt);
                        sb.Replace(match, viewModel.FindAndReplaceViewModel.ReplacePattern, start, length);
                        txt = sb.ToString();
                    }
                    else
                    {
                        
                    }
                }

                return (txt, match, start, length);
            }

            public static (string text, string match, int start, int length) SearchPrevious(SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                while (true)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.FindAndReplaceViewModel.SearchPattern)) return default;

                    var start = offset;

                    text ??= viewModel.Text ?? string.Empty;
                    var searchable = text.Substring(0, start);
                    var pattern = viewModel.FindAndReplaceViewModel.SearchPattern;
                    var matchCase = viewModel.FindAndReplaceViewModel.MatchCase;
                    var useRegex = viewModel.FindAndReplaceViewModel.UseRegex;
                    const SearchDirection direction = SearchDirection.Backward;

                    var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                    if (result.start == -1)
                    {
                        offset = text.Length;
                        continue;
                    }

                    result.text = text;
                    viewModel.SelectText(result.start, result.length);

                    return result;
                }
            }

            public static (string text, string match, int start, int length) SearchNext(SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                while (true)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.FindAndReplaceViewModel.SearchPattern)) return default;

                    text ??= viewModel.Text ?? string.Empty;
                    if (offset == text.Length) offset = -1;
                    var start = offset;
                    var searchable = text.Substring(start + 1);
                    var pattern = viewModel.FindAndReplaceViewModel.SearchPattern;
                    var matchCase = viewModel.FindAndReplaceViewModel.MatchCase;
                    var useRegex = viewModel.FindAndReplaceViewModel.UseRegex;
                    const SearchDirection direction = SearchDirection.Forwards;

                    var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                    if (result.start == -1)
                    {
                        offset = -1;
                        continue;
                    }

                    result.text = text;
                    result.start += start + 1;
                    viewModel.SelectText(result.start, result.length);

                    return result;
                }
            }

            private enum SearchDirection { Forwards, Backward }

            private static (string text, string match, int start, int length) Search(
                SettingsViewModel settings, string searchable, string pattern, bool useRegex, bool matchCase, SearchDirection direction)
            { 
                string result;
                var index = -1;
                var length = 0;

                if (!useRegex)
                {

                    index = direction switch
                    {
                        SearchDirection.Forwards => searchable.IndexOf(pattern,
                            matchCase
                                ? StringComparison.InvariantCulture
                                : StringComparison.InvariantCultureIgnoreCase),
                        SearchDirection.Backward => searchable.LastIndexOf(pattern,
                            matchCase
                                ? StringComparison.InvariantCulture
                                : StringComparison.InvariantCultureIgnoreCase),
                        _ => -1
                    };

                    if (index <= -1)
                    {
                        return (null, null, index, length);
                    }

                    length = pattern.Length;
                    result = searchable.Substring(index, length);

                    settings.Status(
                        $"Matched {pattern}.",
                        TimeSpan.FromSeconds(30), SettingsViewModel.Verbosity.Debug);
                }
                else
                {
                    var regex = new Regex(pattern);

                    var match = direction switch
                    {
                        SearchDirection.Forwards => regex.Match(searchable),
                        SearchDirection.Backward => regex.Matches(searchable).LastOrDefault(),
                        _ => Match.Empty
                    };

                    if (!(match?.Success ?? false))
                    {
                        return (null, null, index, length);
                    }

                    index = match.Index;
                    length = match.Length;
                    result = match.Value;

                    settings.Status(
                        $"Matched {pattern} with {result} at {index}.",
                        TimeSpan.FromSeconds(30), SettingsViewModel.Verbosity.Debug);
                }

                return (null, result, index, length);
            }
        }
    }
}