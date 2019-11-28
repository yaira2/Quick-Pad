using System;
using System.Linq;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;
using Windows.Services.Store;
using QuickPad.Mvvm.Views;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class RateAndReviewCommand : SimpleCommand<SettingsViewModel>
    {
        public RateAndReviewCommand()
        {
            Executioner = async settings =>
            {
                bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9PDLWQHTLSV3"));
            };
        }
    }

    public class ShowFindCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowFindCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowReplace = false;
                documentViewModel.ShowFind = true;
                return Task.CompletedTask;
            };
        }
    }

    public class ShowReplaceCommand : SimpleCommand<DocumentViewModel>
    {
        public ShowReplaceCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowReplace = true;
                return Task.CompletedTask;
            };
        }
    }

    public class HideFindCommand : SimpleCommand<DocumentViewModel>
    {
        public HideFindCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.ShowFind = false;
                return Task.CompletedTask;
            };
        }
    }

    public class FindNextCommand : SimpleCommand<DocumentViewModel>
    {
        public FindNextCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                findAndReplace.FindNext(documentViewModel);
                return Task.CompletedTask;
            };
        }
    }

    public class FindPreviousCommand : SimpleCommand<DocumentViewModel>
    {
        public FindPreviousCommand()
        {
            Executioner = documentViewModel =>
            {
                try
                {
                    var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                    findAndReplace.FindPrevious(documentViewModel);
                    return Task.CompletedTask;
                }
                finally
                {
                    documentViewModel.ReleaseUpdates();
                }
            };
        }
    }

    public class ReplaceNextCommand : SimpleCommand<DocumentViewModel>
    {
        public ReplaceNextCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var result = findAndReplace.ReplaceNext(documentViewModel);
                if (result.start > -1) documentViewModel.Text = result.text;
                return Task.CompletedTask;
            };
        }
    }

    public class ReplaceAllCommand : SimpleCommand<DocumentViewModel>
    {
        public ReplaceAllCommand()
        {
            Executioner = documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var results = findAndReplace.ReplaceAll(documentViewModel);
                if (results.Length > 0) documentViewModel.Text = results.Last().text;
                return Task.CompletedTask;
            };
        }
    }

    public class SelectAllCommand : SimpleCommand<DocumentViewModel>
    {
        public SelectAllCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.SelectText(0, documentViewModel.Text.Length);
                return Task.CompletedTask;
            };
        }
    }

    public class InsertDateTimeCommand : SimpleCommand<DocumentViewModel>
    {
        public InsertDateTimeCommand()
        {
            Executioner = documentViewModel =>
            {
                documentViewModel.SelectedText = DateTimeOffset.Now.LocalDateTime.ToString("g");
                return Task.CompletedTask;
            };
        }
    }
}