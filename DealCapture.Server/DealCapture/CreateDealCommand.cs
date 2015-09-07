using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealCapture.Server.DealCreation
{
    public sealed class CreateDealCommand
    {
        public Guid DealId { get; set; }
        public string Trader { get; set; }
        public string Counterparty { get; set; }
        public List<DealSection> Sections { get; set; }
    }

    public sealed class DealSection
    {
        public string ProductType { get; set; }
        public decimal Notional { get; set; }
        public string Direction { get; set; }
        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliveryUntil { get; set; }
    }
}