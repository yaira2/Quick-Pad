using System;
using QuickPad.Mvvm;

namespace QuickPad.Mvc
{
    public interface IDocumentView : IView
    {
        DocumentViewModel ViewModel { get; set; }

        event Action<IDocumentView> Initialize;
    }
}