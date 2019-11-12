using System;
using QuickPad.MVVM;

namespace QuickPad.MVC
{
    public interface IDocumentView : IView
    {
        DocumentViewModel ViewModel { get; set; }

        event Action<IDocumentView> Initialize;
    }
}