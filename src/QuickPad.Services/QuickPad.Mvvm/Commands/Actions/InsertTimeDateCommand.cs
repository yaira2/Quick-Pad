using System;
using System.Globalization;
using System.Threading.Tasks;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvvm.Commands.Actions
{
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