using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuickPad.Mvvm;

namespace QuickPad.Mvc
{
    public class ApplicationStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<DocumentViewModel, DocumentViewModel>();
            services.AddSingleton<ApplicationController, ApplicationController>();

            // Add additional services here.
        }

        public static void Configure(IConfigurationBuilder configuration)
        {
            // Add additional configuration here.
        }
    }
}
