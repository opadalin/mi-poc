using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Omegapoint.Functions.Server;

public static class HelloServerFunction
{
    [FunctionName("HelloServerFunction")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hello")]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("Server received request");
        // hello
        var token = req.Headers["Authorization"];

        return await Task.FromResult(new OkObjectResult($"Hello client, you sent this token:\n\n {token}"));
    }
}