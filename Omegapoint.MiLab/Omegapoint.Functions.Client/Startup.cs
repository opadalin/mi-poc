using System;
using Invidem.Common.RemoteService;
using Invidem.Common.RemoteService.RetryPolicy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Omegapoint.Functions.Client;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Omegapoint.Functions.Client;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.AddLogging();
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<ITokenService, AzureTokenService>();
        builder.Services.AddSingleton(
            new RemoteServiceRetryPolicyFactoryBuilder()
                .WithDefaultPolicy(new ExponentialRemoteServiceRetryPolicy(0, TimeSpan.FromSeconds(2)))
                .Build());
        
        builder.Services.AddSingleton<IRemoteServer>(provider =>
            new RemoteServer(
                new RemoteServerConfig(
                    baseUri: "https://func-lindis.azurewebsites.net/api/",
                    apiKey: "replace",
                    scope: "api://6ce61091-e3f4-4e04-b7d4-fb007b7cb1ad"),
                provider.GetService<IRemoteServiceRetryPolicyFactory>(),
                provider.GetService<IHttpClientFactory>(),
                provider.GetService<ILoggerFactory>(),
                provider.GetService<ITokenService>()));
    }
}