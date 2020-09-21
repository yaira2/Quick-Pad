using System;
using System.Windows.Input;

namespace QuickPad.Mvvm.Commands
{
    public class SimpleCommand<TValue> : SimpleCommandBase<TValue>, ICommand
    {
        public override void InvokeCanExecuteChanged(TValue value)
        {
            InvokeCanExecuteChanged((object)value);
        }

        public override void Execute(TValue value)
        {
            Executioner?.Invoke(value);
        }

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

        public void InvokeCanExecuteChanged(object sender)
        {
            CanExecuteChanged?.Invoke(sender, EventArgs.Empty);
        }
    }
}