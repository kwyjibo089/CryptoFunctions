using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PortfolioFunction.Models
{
    public class TransactionEntity : TableEntity
    {
        public string Currency { get; set; }
        public double Amount { get; set; }
        public Guid UserId { get; set; }
        public DateTime TransactionTimestamp { get; set; }
        public double Price { get; set; }
        public string Data { get; set; }
    }
}
