using System;
using System.Threading.Tasks;
using System.Web.Http;
using DealCapture.Server.DealCreation;
using DealCapture.Server.EventStore;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace DealCapture.Server.DealCapture
{
    public class DealCaptureController : ApiController
    {
        private readonly IEventStoreClient _eventStoreClient;

        public DealCaptureController(IEventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public async Task<IHttpActionResult> CreateDeal(CreateDealCommand command)
        {
            //De-serialize - already done
            //Validate schema with DataAnnotations/XSD/JSONSchema
            //Apply command to domain
            //return response code;

            var streamName = StreamNames.DealStreamName(command.DealId);
            var eventId = Guid.NewGuid();   //Is this correct? Or should it be cmd.DealId? Or something else?
            var payload = JsonConvert.SerializeObject(command);
            await _eventStoreClient.SaveEvent(streamName, ExpectedVersion.NoStream, eventId, "DealCreated", payload);

            return Ok();
        }
    }

    public static class StreamNames
    {
        private const string DealStreamPrefix = "Deal-";
        public static string DealStreamName(Guid dealId)
        {
            return string.Format("{0}{1}", DealStreamPrefix, dealId);
        }

        public static bool IsDealStream(string streamName)
        {
            return streamName.StartsWith(DealStreamPrefix);
        }
    }
}
