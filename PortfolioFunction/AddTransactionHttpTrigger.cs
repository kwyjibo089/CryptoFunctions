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
using System.Net.Http;

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

            CoinMarketCap coinMarketCap = GetCoinMarketCapConfig(context);

            var coinMarketCapClient = new HttpClient()
            {
                BaseAddress = new System.Uri(coinMarketCap.BaseUrl)                
            };
            coinMarketCapClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", coinMarketCap.ApiKey);
            var result = await coinMarketCapClient.GetAsync(coinMarketCap.Function + transaction.Currency);
            if(result.StatusCode != HttpStatusCode.OK)
                return new BadRequestObjectResult($"Unkown currency {transaction.Currency}");
                       
            req.HttpContext.Response.Headers.Add("API-URL", coinMarketCap.BaseUrl);
            return new OkObjectResult(result.Content);
        }

        private static CoinMarketCap GetCoinMarketCapConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var coinMarketCap = new CoinMarketCap
            {
                BaseUrl = config["CoinMarketCap:URL"],
                ApiKey = config["CoinMarketCap:API-KEY"],
                Function = config["CoinMarketCap:GET-QUOTE"]
            };
            return coinMarketCap;
        }
    }
}
