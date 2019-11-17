using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuickPad.Mvvm.Annotations;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace QuickPad.Mvvm
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
        internal virtual async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!_frozen)
            {
                DispatchedHandler dh = () => { 
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
                };

                try
                {
                    await Dispatch(dh);
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

        protected void Set<TValue>(ref TValue original, TValue value, [CallerMemberName] string propertyName = null)
        {
            if (original?.Equals(value) ?? false) return;

            original = value;

            OnPropertyChanged(propertyName);
        }

        private ConcurrentQueue<string> _updatesQueue = new ConcurrentQueue<string>();

        private bool _frozen = false;

        public void HoldUpdates() 
        {
            _frozen = true;
        }

        public void ReleaseUpdates() 
        {
            _frozen = false;

            while(_updatesQueue.TryDequeue(out string propertyName))
            {
                OnPropertyChanged(propertyName);
            }
        }

        public async Task Dispatch(DispatchedHandler handler)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
        }
    }
}
