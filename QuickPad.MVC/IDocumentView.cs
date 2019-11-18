using System;
using QuickPad.Mvvm;
using QuickPad.MVVM;

namespace QuickPad.Mvc
{
    public interface IDocumentView : IView
    {
        DocumentViewModel ViewModel { get; set; }

        event Action<IDocumentView, QuickPadCommands> Initialize;
    }
}