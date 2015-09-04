using System;
using DealCapture.Client.Dashboards;

namespace DealCapture.Client.Repositories.Dto
{
    public sealed class DealSection
    {
        public string ProductType { get; set; }
        public decimal Notional { get; set; }
        public string Direction { get; set; }
        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliveryUntil { get; set; }
    }
}