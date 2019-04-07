using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortfolioFunction.Models;
using PortfolioFunction.Models.CoinMarketCap;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

            if (transaction.UserId == Guid.Empty)
                return new BadRequestObjectResult("Please pass a userId in the request body");

            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var coinMarketCap = GetCoinMarketCapConfig(context, config);

            var coinMarketCapClient = new HttpClient()
            {
                BaseAddress = new Uri(coinMarketCap.BaseUrl)
            };
            coinMarketCapClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", coinMarketCap.ApiKey);
            var response = await coinMarketCapClient.GetAsync(coinMarketCap.Function + transaction.Currency);
            if (response.StatusCode != HttpStatusCode.OK)
                return new BadRequestObjectResult($"Unkown currency {transaction.Currency}");

            var content = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(content);
                        
            var header = ResponseHeader.FromJson(obj["status"].ToString(Formatting.None));

            var data = obj["data"][transaction.Currency].ToString(Formatting.None);
            var currencyData = CurrencyData.FromJson(data);

            var entity = new TransactionEntity
            {
                PartitionKey = "Transaction",
                RowKey = Guid.NewGuid().ToString(),
                Currency = transaction.Currency,
                Amount = transaction.Amount,
                UserId = transaction.UserId,
                TransactionTimestamp = header.Timestamp,
                Price = currencyData.Quote.Usd.Price,
                Data = data
            };

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("Transactions");
            TableOperation insertOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            
            req.HttpContext.Response.Headers.Add("API-URL", coinMarketCap.BaseUrl);           
            return new OkObjectResult(header);
        }

        private static CoinMarketCapConfiguration GetCoinMarketCapConfig(ExecutionContext context, IConfigurationRoot config)
        {
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
