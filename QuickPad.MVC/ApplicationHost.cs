using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QuickPad.MVC
{
    public class ApplicationHost
    {
        private bool _disposed = false;

        private readonly object _padlock = new object();

        public ApplicationController Controller { get; }
        private readonly IHost _host = null;

        internal ApplicationHost(IHost host)
        {
            _host = host;
            Services = host.Services;
            Controller = Services.GetService<ApplicationController>();
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