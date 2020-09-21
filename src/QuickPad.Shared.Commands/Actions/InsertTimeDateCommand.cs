using QuickPad.Mvvm.ViewModels;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands.Actions
{
    public class InsertTimeDateCommand<TStorageFile, TStream> : SimpleCommand<DocumentViewModel<TStorageFile, TStream>>
        where TStream : class
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