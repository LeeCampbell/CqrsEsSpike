using System;
using System.Collections.Generic;

namespace DealCapture.Client.Repositories.Dto
{
    public sealed class CreateDealCommand
    {
        public Guid DealId { get; set; }
        public string Trader { get; set; }
        public string Counterparty { get; set; }
        public List<DealSection> Sections { get; set; }
    }
}