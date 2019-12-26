using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
                SettingsViewModel settings, string text, DocumentViewModel viewModel)
            {
                var count = 0;
                var offset = -1;
                viewModel.SelectText(0, 0, false);

                (string text, string match, int start, int length) current = default;

                var results = new List<(string text, string match, int start, int length)>();

                // ReSharper disable once ComplexConditionExpression
                while ((current = ReplaceNext(settings, text, offset, viewModel)).start > -1)
                {
                    text = current.text;
                    results.Add(current);
                    offset = current.start + 1;
                    count++;
                }

                settings.Status($"Made {count} replacements."
                    , TimeSpan.FromMinutes(1)
                    , SettingsViewModel.Verbosity.Release);

                return results.ToArray();
            }

            public static (string text, string match, int start, int length) ReplaceNext(
                SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                var (txt, match, start, length) = SearchNext(settings, text, offset, viewModel);

                if (length <= 0 || match == null) return (txt, match, start, length);

                var pattern = viewModel.FindAndReplaceViewModel.ReplacePattern;

                viewModel.SelectText(start, length, false);

                var sb = new StringBuilder(txt);

                if (viewModel.FindAndReplaceViewModel.UseRegex)
                {
                    var regex = new Regex(viewModel.FindAndReplaceViewModel.SearchPattern);
                    var matches = regex.Match(match);
                    for (var i = matches.Groups.Count; i >= 2; --i)
                    {
                        var plug = $"${i-1}";
                        pattern = pattern.Replace(plug, matches.Groups[i-1].Value);
                    }
                }

                
                sb.Replace(match, pattern, start, length);

                txt = sb.ToString();
                viewModel.SelectedText = pattern;
                viewModel.SelectText(start, pattern.Length, true);

                return (txt, match, start, length);
            }

            public static (string text, string match, int start, int length) SearchPrevious(SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                if (string.IsNullOrWhiteSpace(viewModel.FindAndReplaceViewModel.SearchPattern)) return (text, null, -1, 0);

                var pattern = viewModel.FindAndReplaceViewModel.SearchPattern;
                var matchCase = viewModel.FindAndReplaceViewModel.MatchCase;
                var useRegex = viewModel.FindAndReplaceViewModel.UseRegex;
                const SearchDirection direction = SearchDirection.Backward;
                var canContinue = true;

                while (canContinue)
                {
                    if(offset == -1 || useRegex) canContinue = false;

                    var start = offset;

                    text ??= viewModel.Text ?? string.Empty;
                    var searchable = text.Substring(0, start);

                    var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                    if (result.start == -1)
                    {
                        offset = text.Length;
                        continue;
                    }

                    result.text = text;
                    viewModel.SelectText(result.start, result.length, false);

                    return result;
                }

                return (text, null, -1, 0);
            }

            public static (string text, string match, int start, int length) SearchNext(SettingsViewModel settings, string text, int offset, DocumentViewModel viewModel)
            {
                if (string.IsNullOrWhiteSpace(viewModel.FindAndReplaceViewModel.SearchPattern)) return (text, null, -1, 0);

                var pattern = viewModel.FindAndReplaceViewModel.SearchPattern;
                var matchCase = viewModel.FindAndReplaceViewModel.MatchCase;
                var useRegex = viewModel.FindAndReplaceViewModel.UseRegex;
                const SearchDirection direction = SearchDirection.Forwards;
                var canContinue = true;

                while (canContinue)
                {
                    if (offset == -1 || useRegex) canContinue = false;

                    text ??= viewModel.Text ?? string.Empty;
                    if (offset == text.Length) offset = -1;
                    var start = Math.Min(offset, text.Length - 1);
                    var searchable = text.Substring(start + 1);

                    var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                    if (result.start == -1)
                    {
                        offset = -1;
                        continue;
                    }

                    result.text = text;
                    result.start += start + 1;
                    viewModel.SelectText(result.start, result.length, false);

                    return result;
                }

                return (text, null, -1, 0);
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