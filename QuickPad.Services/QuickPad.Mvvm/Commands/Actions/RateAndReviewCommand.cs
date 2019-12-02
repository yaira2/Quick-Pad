using System;
using System.Linq;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;
using Windows.Services.Store;
using QuickPad.Mvvm.Views;
using System.Globalization;

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
            Executioner = async documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                await findAndReplace.FindNext(documentViewModel);
            };
        }
    }

    public class FindPreviousCommand : SimpleCommand<DocumentViewModel>
    {
        public FindPreviousCommand()
        {
            Executioner = async documentViewModel =>
            {
                try
                {
                    var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                    await findAndReplace.FindPrevious(documentViewModel);
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
            Executioner = async documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var result = await findAndReplace.ReplaceNext(documentViewModel);
                if (result.start > -1) documentViewModel.Text = result.text;
            };
        }
    }

    public class ReplaceAllCommand : SimpleCommand<DocumentViewModel>
    {
        public ReplaceAllCommand()
        {
            Executioner = async documentViewModel =>
            {
                var findAndReplace = documentViewModel.FindAndReplaceViewModel;
                var results = await findAndReplace.ReplaceAll(documentViewModel);
                if (results.Length > 0) documentViewModel.Text = results.Last().text;
            };
        }
    }

    public class SelectAllCommand : SimpleCommand<DocumentViewModel>
    {
        public SelectAllCommand()
        {
            Executioner = async documentViewModel =>
            {
                await documentViewModel.SelectText(0, documentViewModel.Text.Length);
            };
        }
    }

    public class InsertTimeDateCommand : SimpleCommand<DocumentViewModel>
    {
        public InsertTimeDateCommand()
        {
            Executioner = documentViewModel =>
            {
                var current = CultureInfo.CurrentUICulture.DateTimeFormat;
                documentViewModel.SelectedText = DateTime.Now.ToString(current.ShortTimePattern) + " " + DateTime.Now.ToString(current.ShortDatePattern);
                return Task.CompletedTask;
            };
        }
    }
}