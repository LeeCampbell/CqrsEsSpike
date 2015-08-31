using System;

namespace DealCapture.Client.Repositories.Dto
{
    public sealed class DealSection
    {
        public string ProductType { get; set; }
        public decimal Notional { get; set; }
        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliverUntil { get; set; }
    }
}