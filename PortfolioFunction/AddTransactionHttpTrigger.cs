using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PortfolioFunction.Models;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace PortfolioFunction
{
    public static class AddTransactionHttpTrigger
    {
        [FunctionName("AddTransactionHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation($"C# HTTP trigger function processed a request: {requestBody}");

            var transaction = Transaction.FromJson(requestBody);

            if (string.IsNullOrWhiteSpace(transaction.Currency))
                return new BadRequestObjectResult("Please pass a currency in the request body");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var url = config["CoinMarketCap:URL"];

            req.HttpContext.Response.Headers.Add("API-URL", url);

            return new OkResult();
        }
    }
}
