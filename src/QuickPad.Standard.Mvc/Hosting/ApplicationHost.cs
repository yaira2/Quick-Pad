using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickPad.Mvc.Hosting
{
    public class ApplicationHost<TStorageFile, TStream, TDocumentManager>
        where TDocumentManager : DocumentManager<TStorageFile, TStream, TDocumentManager>
        where TStream : class
    {
        private bool _disposed = false;

        private readonly object _padlock = new object();

        public ApplicationController<TStorageFile, TStream, TDocumentManager> Controller { get; }
        private readonly IHost _host = null;

        internal ApplicationHost(IHost host)
        {
            _host = host;
            Services = host.Services;
            Controller = Services.GetService<ApplicationController<TStorageFile, TStream, TDocumentManager>>();
        }

        public void Dispose()
        {
            lock (_padlock)
            {
                if (_disposed) return;

                _host.Dispose();
                _disposed = true;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public IServiceProvider Services { get; }
    }
}