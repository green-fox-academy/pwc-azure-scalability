using System;
using InvoiceProcessor.Common.Services;
using InvoiceProcessor.Common.Transformations;
using InvoiceProcessor.Functions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(InvoiceProcessor.Functions.Startup))]

namespace InvoiceProcessor.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);

            builder.ConfigurationBuilder.AddUserSecrets(typeof(Startup).Assembly);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddHttpClient()
                .AddScoped<IFakeExternalServiceClient, FakeExternalServiceClient>()
                .AddScoped<IXslTransformationService, XslTransformationService>()
                .AddScoped<IInvoiceSetSerializer, InvoiceSetSerializer>();

            builder.Services.AddHttpClient(
                ClientNames.FakeExternalServiceHttpClient,
                (serviceProvider, client) =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    var externalServiceBaseUrl = configuration.GetValue<Uri>(SettingNames.ExternalServiceBaseUrl);
                    client.BaseAddress = externalServiceBaseUrl;
                });
        }
    }
}
