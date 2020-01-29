using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace QuickPad.Mvvm.ViewModels
{
    public abstract class ViewModel<TStorageFile, TStream> : INotifyPropertyChanged
        where TStream : class
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        private bool _frozen;
        private bool _blockUpdates;
        
        protected ILogger Logger { get; }
        
        [JsonIgnore]
        public IApplication<TStorageFile, TStream> App { get; }

        protected ViewModel(ILogger logger, IApplication<TStorageFile, TStream> app)
        {
            Logger = logger;
            App = app;
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_blockUpdates) return;

            if (!_frozen)
            {
                void DispatchedHandler()
                {
                    try
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                    catch 
                    {
                        // We don't want an exception taking down the timer.   
                    }
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

        public virtual bool Set<TValue>(ref TValue original, TValue value, [CallerMemberName] string propertyName = null, bool notify = true)
        {
            if (original?.Equals(value) ?? false) return false;

            original = value;

            if(notify) OnPropertyChanged(propertyName);

            return true;
        }

        private readonly ConcurrentQueue<string> _updatesQueue = new ConcurrentQueue<string>();

        public void HoldUpdates() 
        {
            _frozen = true;
        }

        public void ReleaseUpdates() 
        {
            _frozen = false;
            _blockUpdates = false;

            while(_updatesQueue.TryDequeue(out var propertyName))
            {
                OnPropertyChanged(propertyName);
            }
        }

        public void Dispatch(Action handler)
        {
            App.TryEnqueue(handler);
        }

        public Task<TResult> Dispatch<TResult>(Func<TResult> handler)
        {
            return App.AwaitableRunAsync<TResult>(handler);
        }

        public void BlockUpdates()
        {
            _blockUpdates = true;
        }

    }
}
