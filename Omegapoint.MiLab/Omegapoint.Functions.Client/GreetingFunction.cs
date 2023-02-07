using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Omegapoint.Functions.Client;

public class GreetingFunction
{
    private readonly HttpClient _httpClient;

    public GreetingFunction(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://func-lindis.azurewebsites.net/api/");
        //_httpClient.BaseAddress = new Uri("http://localhost:7071/api/");
    }

    [FunctionName("GreetingFunction")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "greeting")]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("Entered greeting function");
        var scopes = new[]
        {
            "api://6ce61091-e3f4-4e04-b7d4-fb007b7cb1ad"
        };

        var managedIdentityAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions()
        {
            ExcludeSharedTokenCacheCredential = false
        });
        var accessToken = await managedIdentityAzureCredential.GetTokenAsync(new TokenRequestContext(scopes));

        var token = accessToken.Token;
        
        log.LogInformation("Received token from DefaultAzureCredential");
        
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://func-lindis.azurewebsites.net/api/hello"),
            Headers = {{"Authorization", token},
            {"x-functions-key", "replace"}}
        };

        var response = await _httpClient.SendAsync(httpRequest);
        log.LogInformation("Greeting is sent to downstream server");

        if (!response.IsSuccessStatusCode)
        {
            return new ObjectResult(HttpStatusCode.InternalServerError +
                                    ": Received unsuccessful response from server");
        }

        var content = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(new OkObjectResult($"Response from server: {content}"));
    }
}