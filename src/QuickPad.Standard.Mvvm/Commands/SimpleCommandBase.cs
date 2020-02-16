using System;
using System.Threading.Tasks;

namespace QuickPad.Mvvm.Commands
{
    public abstract class SimpleCommandBase<TValue> : ISimpleCommand<TValue>
    {
        public Func<TValue, bool> CanExecuteEvaluator { get; set; }
        public Func<TValue, Task> Executioner { get; set; }

        public abstract void InvokeCanExecuteChanged(TValue value);

        public abstract void Execute(TValue value);
    }
}