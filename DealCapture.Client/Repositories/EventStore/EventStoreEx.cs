using System;
using System.Reactive.Linq;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace DealCapture.Client.Repositories.EventStore
{
    internal static class EventStoreEx
    {
        public static void SaveEvent<T>(this IEventStoreClient eventStoreClient, string streamName, int expectedVersion, Guid eventId, T data)
        {
            eventStoreClient.SaveEvent(streamName, 
                expectedVersion, 
                eventId, 
                data.GetType().Name, 
                JsonConvert.SerializeObject(data));
        }

        public static IObservable<T> GetEvents<T>(this IEventStoreClient eventStoreClient, string streamName)
        {
            return eventStoreClient.GetEvents(streamName)
                .Select(re => Encoding.UTF8.GetString(re.OriginalEvent.Data))
                .Select(JsonConvert.DeserializeObject<T>);
        }

        public static T Deserialize<T>(this RecordedEvent recordedEvent)
        {
            var data = recordedEvent.Data;
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}