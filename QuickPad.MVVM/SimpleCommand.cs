using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;

namespace QuickPad.MVVM
{
    public class SimpleCommand<TValue> : ICommand
    {
        public Func<TValue, bool> CanExecuteEvaluator { get; set; }
        public bool CanExecute(object parameter)
        {
            if(parameter is TValue value)
            {
                return CanExecuteEvaluator?.Invoke(value) ?? true;
            }

            return true;
        }

        public Func<TValue, Task> Executioner { get; set; }

        public void Execute(object parameter)
        {
            if(parameter is TValue value)
            {
                Executioner?.Invoke(value);
            }

            Executioner?.Invoke(default);
        }

        public event EventHandler CanExecuteChanged;
    }
}
