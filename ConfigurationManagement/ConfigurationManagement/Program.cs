using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
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
                    webBuilder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddAzureAppConfiguration(appConfigOptions =>
                        {
                            appConfigOptions.Connect(appConfigurationConnectionString);
                            //appConfigOptions.Select(KeyFilter.Any, LabelFilter.Null);
                            appConfigOptions.Select(KeyFilter.Any, "Stage"); //context.HostingEnvironment.EnvironmentName
                            appConfigOptions.ConfigureRefresh(appConfigRefresherOption =>
                            {
                                appConfigRefresherOption.SetCacheExpiration(TimeSpan.FromSeconds(2));
                                appConfigRefresherOption.Register("PWC:Refresh", true);
                            });
                            appConfigOptions.UseFeatureFlags(featureFlagOptions =>
                            {
                                //featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(1);
                                featureFlagOptions.Select(KeyFilter.Any, LabelFilter.Null);
                            });
                        }, false);
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
