using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigurationManagement
{
    public class Program
    {
        private static string appConfigurationConnectionString = "Endpoint=https://appcs-fg-pwc.azconfig.io;Id=pi5x-l9-s0:SZLlhHyJ9Nz2MpAl04cU;Secret=CQ+mlfQqkzfZv4XMMxgigJ/seeXMKwNsqW/rM3wmtuE=";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration(config => 
                    {
                        config.AddAzureAppConfiguration(appConfigurationConnectionString);
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
