using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealCapture.Client.Repositories;

namespace DealCapture.Client.DealEnrichment
{
    class DealEnrichmentViewModel
    {
        private readonly IDealRepository _dealRepo;
        private readonly Guid _dealId;

        public DealEnrichmentViewModel(IDealRepository dealRepo, Guid dealId)
        {
            _dealRepo = dealRepo;
            _dealId = dealId;
        }

        public IDisposable Start()
        {
            //Get head
            //_dealRepo.
            //Play up until head
            //Alert user if the form is dirty and a new evt arrives.
            return null;
            
        }
    }
}
