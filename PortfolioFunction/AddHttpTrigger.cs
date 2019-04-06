using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PortfolioFunction.Models;

namespace PortfolioFunction
{
    public static class AddHttpTrigger
    {
        [FunctionName("AddHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation($"C# HTTP trigger function processed a request: {requestBody}");                        
            
            var transaction = Transaction.FromJson(requestBody);
            
            if(string.IsNullOrWhiteSpace( transaction.Currency))
                new BadRequestObjectResult("Please pass a currency in the request body");
                       
            return new OkResult();
        }
    }
}
