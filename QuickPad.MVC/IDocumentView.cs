using System;
using QuickPad.MVVM.Commands;
using QuickPad.MVVM.ViewModels;

namespace QuickPad.MVC
{
    public interface IDocumentView : IView
    {
        DocumentViewModel ViewModel { get; set; }

        event Action<IDocumentView, QuickPadCommands> Initialize;
    }
}