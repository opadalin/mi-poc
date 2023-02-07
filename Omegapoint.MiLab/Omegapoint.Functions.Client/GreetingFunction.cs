using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Invidem.Common.RemoteService;
using Invidem.Common.RemoteService.RetryPolicy;

namespace Omegapoint.Functions.Client;

public class GreetingFunction
{
    private readonly ILogger<GreetingFunction> _logger;

    private readonly ITokenService _tokenService;
    private readonly IRemoteServer _remoteServer;


    public GreetingFunction(
        ITokenService tokenService,
        IRemoteServer remoteServer,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(tokenService);
        ArgumentNullException.ThrowIfNull(remoteServer);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<GreetingFunction>();
        _tokenService = tokenService;
        _remoteServer = remoteServer;
        // _httpClient.BaseAddress = new Uri("https://func-lindis.azurewebsites.net/api/");
    }

    [FunctionName("GreetingFunction")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "greeting")]
        HttpRequest req)
    {
        //const string scopes = "api://6ce61091-e3f4-4e04-b7d4-fb007b7cb1ad";

        _logger.LogInformation("Entered greeting function");
        //var token = await _tokenService.GetTokenAsync(scopes, _logger);

        _logger.LogInformation("Greeting is sent to downstream server");
        var responseFromRemoteServer = await _remoteServer.HelloServer("hello", "replace");
        
        var content = await responseFromRemoteServer.Content.ReadAsStringAsync();

        _logger.LogInformation("Received token from DefaultAzureCredential, {Content}", content);
        
        if (!responseFromRemoteServer.IsSuccessStatusCode)
        {
            _logger.LogInformation("Status code from server: {Code}, Message: {Message}",
                responseFromRemoteServer.StatusCode, content);
            return new ObjectResult(responseFromRemoteServer.StatusCode +
                                    $": Received unsuccessful response from server  Message: {content}" );
        }

        return await Task.FromResult(new OkObjectResult($"Response from server: {content}"));
    }
}

public interface IRemoteServer
{
    Task<HttpResponseMessage> HelloServer(string requestUri, string functionsKey);
}

public class RemoteServer : RemoteServiceBase, IRemoteServer
{
    public RemoteServer(
        RemoteServiceConfigurationBase configuration,
        IRemoteServiceRetryPolicyFactory retryPolicyFactory,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        ITokenService tokenService)
        : base(
            configuration,
            retryPolicyFactory.CreatePolicy<RemoteServer>(),
            httpClientFactory.CreateClient(),
            loggerFactory.CreateLogger<RemoteServer>(),
            tokenService)
    {
    }

    public async Task<HttpResponseMessage> HelloServer(string requestUri, string functionsKey)
    {
        return await GetAsync(requestUri, CancellationToken.None, new Dictionary<string, string>()
        {
            {"x-functions-key", functionsKey}
        });
    }
}

public class RemoteServerConfig : RemoteServiceConfigurationBase
{
    public RemoteServerConfig(
        string baseUri,
        string apiKey,
        string scope)
        : base(baseUri, apiKey, scope)
    {
    }
}