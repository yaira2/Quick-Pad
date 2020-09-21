using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace QuickPad.Mvc.Hosting
{
    public static class Extensions
    {
        public static IHostBuilder UseHostedService<T>(this IHostBuilder hostBuilder)
            where T : class, IHostedService, IDisposable
        {
            return hostBuilder.ConfigureServices(services =>
                services.AddHostedService<T>());
        }

        public static ApplicationHost<TStorageFile, TStream, TDocumentManager> BuildApplicationHost<TStorageFile, TStream, TDocumentManager>(this IHostBuilder hostBuilder)
            where TDocumentManager : DocumentManager<TStorageFile, TStream, TDocumentManager>
            where TStream : class
        {
            return new ApplicationHost<TStorageFile, TStream, TDocumentManager>(hostBuilder.Build());
        }
    }
}