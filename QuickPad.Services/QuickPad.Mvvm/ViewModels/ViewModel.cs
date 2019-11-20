using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Helpers;
using QuickPad.Mvvm.Properties;

namespace QuickPad.Mvvm.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private ILogger Logger { get; }

        public ViewModel(ILogger logger)
        {
            Logger = logger;
        }

        [NotifyPropertyChangedInvocator]
        internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!_frozen)
            {
                void DispatchedHandler()
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }

                try
                {
                    Dispatch(DispatchedHandler);
                }
                catch(Exception ex)
                {
                    Logger.LogError(new EventId(), ex, "Error dispatching Property Changed Event.");
                }
            }
            else
            {
                _updatesQueue.Enqueue(propertyName);
            }
        }        

        protected virtual bool Set<TValue>(ref TValue original, TValue value, [CallerMemberName] string propertyName = null)
        {
            if (original?.Equals(value) ?? false) return false;

            original = value;

            OnPropertyChanged(propertyName);

            return true;
        }

        private readonly ConcurrentQueue<string> _updatesQueue = new ConcurrentQueue<string>();

        private bool _frozen;

        public void HoldUpdates() 
        {
            _frozen = true;
        }

        public void ReleaseUpdates() 
        {
            _frozen = false;

            while(_updatesQueue.TryDequeue(out var propertyName))
            {
                OnPropertyChanged(propertyName);
            }
        }

        public void Dispatch(DispatcherQueueHandler handler)
        {
            CoreApplication.MainView.DispatcherQueue.TryEnqueue(handler);
        }
    }
}
