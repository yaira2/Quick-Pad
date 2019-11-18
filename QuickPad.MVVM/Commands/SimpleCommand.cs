using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickPad.MVVM.Commands
{
    public class SimpleCommand<TValue> : ICommand
    {
        public Func<TValue, bool> CanExecuteEvaluator { get; set; }

        public Func<TValue, Task> Executioner { get; set; }

        public bool CanExecute(object parameter)
        {
            if (parameter is TValue value) return CanExecuteEvaluator?.Invoke(value) ?? true;

            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TValue value)
            {
                Executioner?.Invoke(value);
                return;
            }

            Executioner?.Invoke(default);
        }

        public event EventHandler CanExecuteChanged;

        protected void InvokeCanExecuteChanged(object sender)
        {
            CanExecuteChanged?.Invoke(sender, EventArgs.Empty);
        }
    }
}