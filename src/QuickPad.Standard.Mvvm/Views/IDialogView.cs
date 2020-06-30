using System;

namespace QuickPad.Mvvm.Views
{
    public interface IDialogView : IView
    {
        event Action Closed;
    }
}