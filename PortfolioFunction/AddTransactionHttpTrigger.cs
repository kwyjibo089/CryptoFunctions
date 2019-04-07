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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using PortfolioFunction.Models.CoinMarketCap;

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

            var coinMarketCap = GetCoinMarketCapConfig(context);

            var coinMarketCapClient = new HttpClient()
            {
                BaseAddress = new System.Uri(coinMarketCap.BaseUrl)                
            };
            coinMarketCapClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", coinMarketCap.ApiKey);
            var response = await coinMarketCapClient.GetAsync(coinMarketCap.Function + transaction.Currency);
            if(response.StatusCode != HttpStatusCode.OK)
                return new BadRequestObjectResult($"Unkown currency {transaction.Currency}");

            var content = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(content);

            var header = ResponseHeader.FromJson(obj["status"].ToString(Formatting.None));

            var responseData = ResponseData.FromJson(obj["data"][transaction.Currency].ToString(Formatting.None));

            req.HttpContext.Response.Headers.Add("API-URL", coinMarketCap.BaseUrl);
            req.HttpContext.Response.Headers.Add("API-TIMESTAMP", header.Timestamp.ToString());
            return new OkObjectResult(obj["data"][transaction.Currency]);
        }

        private static CoinMarketCapConfiguration GetCoinMarketCapConfig(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var coinMarketCap = new CoinMarketCapConfiguration
            {
                BaseUrl = config["CoinMarketCap:URL"],
                ApiKey = config["CoinMarketCap:API-KEY"],
                Function = config["CoinMarketCap:GET-QUOTE"]
            };
            return coinMarketCap;
        }
    }
}
