using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationManagement
{
    public class Program
    {
        private static string appConfigurationConnectionString = "Endpoint=https://appcs-fg-pwc.azconfig.io;Id=pi5x-l9-s0:SZLlhHyJ9Nz2MpAl04cU;Secret=CQ+mlfQqkzfZv4XMMxgigJ/seeXMKwNsqW/rM3wmtuE=";
        private static string queueConnectionString = "DefaultEndpointsProtocol=https;AccountName=stgfevents;AccountKey=59/PEl9dxJTMrRlH7J4huOcHC5BXeN6czh9CIN240+NGOHZYed4qYqRz1IZD3ef9dZGwjUqHlNKyERi+wihl7Q==;EndpointSuffix=core.windows.net";
        private static string queueName = "configchanges";

        private static IConfigurationRefresher _refresher = null;
        private static Timer _refresherTimer;

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

                            _refresher = appConfigOptions.GetRefresher();

                            //RegisterRefreshEventHandler();

                            appConfigOptions.ConfigureRefresh(appConfigRefresherOption =>
                            {
                                appConfigRefresherOption.SetCacheExpiration(TimeSpan.FromSeconds(2));
                                appConfigRefresherOption.Register("PWC:Refresh", true);
                            });
                            appConfigOptions.UseFeatureFlags(featureFlagOptions =>
                            {
                                featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(1);
                                featureFlagOptions.Select(KeyFilter.Any, LabelFilter.Null);
                            });
                        }, false);
                    });
                    webBuilder.UseStartup<Startup>();
                });

        private static void RegisterRefreshEventHandler()
        {
            _refresherTimer = new Timer(RefreshFromQueue, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private static void RefreshFromQueue(object state)
        {
            var queueClient = new QueueClient(queueConnectionString, queueName);
            var receivedMessages = queueClient.ReceiveMessages();

            if (receivedMessages.Value.Count() > 0)
            {
                _refresher.SetDirty(TimeSpan.FromSeconds(1));
            }

            foreach (QueueMessage message in receivedMessages.Value)
            {
                queueClient.DeleteMessage(message.MessageId, message.PopReceipt);
            }
        }
    }
}
