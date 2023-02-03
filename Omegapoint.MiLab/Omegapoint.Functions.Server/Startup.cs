using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omegapoint.Functions.Server;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Omegapoint.Functions.Server;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddLogging();
        builder.Services.AddHttpClient();
    }
}