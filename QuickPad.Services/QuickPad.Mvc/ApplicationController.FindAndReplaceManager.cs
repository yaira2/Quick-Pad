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
            public static async Task<(string text, string match, int start, int length)[]> ReplaceAll(SettingsViewModel settings, string text, DocumentViewModel arg)
            {
                await arg.SelectText(0, 0);
                (string text, string match, int start, int length) current = default;

                var results = new List<(string text, string match, int start, int length)>();

                while ((current = await ReplaceNext(settings, text, arg)).start > -1)
                {
                    text = current.text;
                    results.Add(current);
                }

                return results.ToArray();
            }

            public static async Task<(string text, string match, int start, int length)> ReplaceNext(SettingsViewModel settings, string text, DocumentViewModel arg)
            {
                var (txt, match, start, length) = await SearchNext(settings, text, arg);

                if (start > -1)
                {
                    //arg.SelectText(match.start, match.length);
                    //arg.SelectedText = arg.FindAndReplaceViewModel.ReplacePattern;
                    
                    var sb = new StringBuilder(txt);
                    sb.Replace(match, arg.FindAndReplaceViewModel.ReplacePattern, start, length);
                    txt = sb.ToString();
                }

                return (txt, match, start, length);
            }

            public static async Task<(string text, string match, int start, int length)> SearchPrevious(SettingsViewModel settings, string text, DocumentViewModel arg)
            {
                if (string.IsNullOrWhiteSpace(arg.FindAndReplaceViewModel.SearchPattern))
                    return default;

                var (start, _) = arg.CurrentPosition;

                var txt = text ?? arg.Text;
                var searchable = txt.Substring(0, start);
                var pattern = arg.FindAndReplaceViewModel.SearchPattern;
                var matchCase = arg.FindAndReplaceViewModel.MatchCase;
                var useRegex = arg.FindAndReplaceViewModel.UseRegex;
                const SearchDirection direction = SearchDirection.Backward;

                var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                if (result.start == -1) return result;

                result.text = txt;
                await arg.SelectText(result.start, result.length);

                return result;
            }

            public static async Task<(string text, string match, int start, int length)> SearchNext(SettingsViewModel settings, string text,
                DocumentViewModel arg)
            {
                if (string.IsNullOrWhiteSpace(arg.FindAndReplaceViewModel.SearchPattern))
                    return default;

                var (start, _) = arg.CurrentPosition;

                var txt = text ?? arg.Text;
                var searchable = txt.Substring(start + 1);
                var pattern = arg.FindAndReplaceViewModel.SearchPattern;
                var matchCase = arg.FindAndReplaceViewModel.MatchCase;
                var useRegex = arg.FindAndReplaceViewModel.UseRegex;
                const SearchDirection direction = SearchDirection.Forwards;

                var result = Search(settings, searchable, pattern, useRegex, matchCase, direction);

                if (result.start == -1) return result;

                result.text = txt;
                result.start += start + 1;
                await arg.SelectText(result.start, result.length);

                return result;
            }

            private enum SearchDirection { Forwards, Backward }

            private static (string text, string match, int start, int length) Search(SettingsViewModel settings, string searchable, string pattern, bool useRegex, bool matchCase, SearchDirection direction)
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
                        settings.Status($"{pattern} not find in the {direction} direction."
                            , TimeSpan.FromSeconds(30)
                            , SettingsViewModel.Verbosity.Debug);

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
                        settings.Status($"{pattern} not find in the {direction} direction."
                            , TimeSpan.FromSeconds(30)
                            , SettingsViewModel.Verbosity.Debug);

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