using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ResilienceSimulator.Account;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResilienceSimulator
{
    class Program
    {
        private static ServiceProvider serviceProvider;

        static async Task Main(string[] args)
        {
            Setup();

            var accountService = serviceProvider.GetService<IAccountService>();
            var logger = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        var balance = await accountService.GetCurrentBalanceAsync();
                        Console.WriteLine($"Current balance is: {balance}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"#{i}  Exception: {ex.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception: {ex.GetType().Name}");
            }
        }

        private static void Setup()
        {
            //setup our DI
            serviceProvider = new ServiceCollection()
                .AddLogging(configure =>
                {
                    configure.AddConsole(configure =>
                    {
                        configure.TimestampFormat = "[HH:mm:ss.fff] ";
                    });
                })
                .AddMemoryCache()
                .AddSingleton<IAccountService, CircuitBreakerAccountService>()
                .BuildServiceProvider();
        }

        private static void Wait()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to exit ...");
            Console.ReadKey();
        }
    }
}
