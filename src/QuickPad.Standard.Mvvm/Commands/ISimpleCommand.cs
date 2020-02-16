using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands
{
    public interface ISimpleCommand<TValue>
    {
        Func<TValue, bool> CanExecuteEvaluator { get; set; }
        Func<TValue, Task> Executioner { get; set; }

        void InvokeCanExecuteChanged(TValue value);

        void Execute(TValue value);
    }
}