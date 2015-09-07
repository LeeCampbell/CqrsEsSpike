using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace DealCapture.Server.EventStore
{
    public interface IEventStoreClient
    {
        IObservable<ResolvedEvent> AllEvents();
        IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null);

        Task<int> GetHeadVersion(string streamName);
        Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData,
            string jsonMetaData = null);

        Task SaveBatch(string streamName, int expectedVersion, string eventType, string[] jsonData);
    }
}