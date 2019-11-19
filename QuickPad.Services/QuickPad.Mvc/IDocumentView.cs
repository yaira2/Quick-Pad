using System;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.ViewModels;

namespace QuickPad.Mvc
{
    public interface IDocumentView : IView
    {
        DocumentViewModel ViewModel { get; set; }

        event Action<IDocumentView, QuickPadCommands> Initialize;
    }
}