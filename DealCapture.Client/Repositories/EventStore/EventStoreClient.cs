using System;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.SystemData;

namespace DealCapture.Client.Repositories.EventStore
{
    public sealed class EventStoreClient : IEventStoreClient, IDisposable
    {
        private static readonly UserCredentials AdminUserCredentials = new UserCredentials("admin", "changeit");
        private readonly Lazy<Task<IEventStoreConnection>> _conn;
        private bool _hasConnectionBegun;

        public EventStoreClient()
        {
            _conn = new Lazy<Task<IEventStoreConnection>>(() =>
            {
                _hasConnectionBegun = true;
                return Connect();
            });
        }

        public IObservable<ResolvedEvent> GetEvents(string streamName, int? fromVersion = null)
        {
            return Observable.Create<ResolvedEvent>(async o =>
            {
                var conn = await _conn.Value;

                Action<EventStoreCatchUpSubscription, ResolvedEvent> callback = (arg1, arg2) => o.OnNext(arg2);

                var subscription = conn.SubscribeToStreamFrom(streamName, fromVersion, true, callback);

                return Disposable.Create(subscription.Stop);
            });
        }

        public async Task<int> GetHeadVersion(string streamName)
        {
            var conn = await _conn.Value;

            var slice = await conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
            if (slice.Status == SliceReadStatus.Success && slice.Events.Length == 1)
            {
                return slice.Events[0].OriginalEvent.EventNumber;
            }
            if (slice.Status == SliceReadStatus.StreamNotFound)
            {
                return ExpectedVersion.NoStream;//- 1;
            }
            throw new StreamDeletedException(streamName);
        }

        public async Task SaveEvent(string streamName, int expectedVersion, Guid eventId, string eventType, string jsonData, string jsonMetaData = null)
        {
            Console.WriteLine("SaveEvent(" + streamName + ", " + expectedVersion + ", " + eventId + ", " + eventType + ")");
            var payload = Encoding.UTF8.GetBytes(jsonData);
            var metadata = jsonMetaData == null ? null : Encoding.UTF8.GetBytes(jsonMetaData);
            try
            {
                var conn = await _conn.Value;
                await conn.AppendToStreamAsync(streamName, expectedVersion, new EventData(eventId, eventType, true, payload, metadata));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to SaveEvent({0}, {1}, {2}, {3}) : {4}", streamName, expectedVersion, eventId, eventType, ex);
                throw;
            }
        }

        public async Task SaveBatch(string streamName, int expectedVersion, string eventType, string[] jsonData)
        {
            Console.WriteLine("SaveBatch({0}, {1}, {2}, jsonData[{3}])", streamName, expectedVersion, eventType, jsonData.Length);
            var events = jsonData.Select(Encoding.UTF8.GetBytes)
                .Select(bin => new EventData(Guid.NewGuid(), eventType, true, bin, null));

            try
            {
                var conn = await _conn.Value;
                await conn.AppendToStreamAsync(streamName, expectedVersion, events);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to SaveBatch({0}, {1}, {2}, jsonData[{3}]) : {4}", streamName, expectedVersion, eventType, jsonData.Length, ex);
                throw;
            }
        }

        public IObservable<ResolvedEvent> AllEvents()
        {
            return Observable.Create<ResolvedEvent>(
                async obs =>
                {
                    var resources = new CompositeDisposable();
                    var conn = await _conn.Value;
                    resources.Add(conn);
                    try
                    {
                        var subscription = conn.SubscribeToAllFrom(null, false,
                            (eventStoreCatchUpSubscription, resolvedEvent) => obs.OnNext(resolvedEvent),
                            (eventStoreCatchUpSubscription) => { },
                            (eventStoreCatchUpSubscription, dropReason, exception) =>
                            {
                                if (dropReason != SubscriptionDropReason.UserInitiated)
                                    Console.WriteLine("Subscription was dropped '{0}' - Error: {1}", dropReason,
                                        exception);
                            },
                            AdminUserCredentials);

                        resources.Add(Disposable.Create(subscription.Stop));
                        return resources;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error subscribing to all events : {0}", e);
                        conn.Dispose();
                        throw;
                    }
                });

        }

        private static async Task<IEventStoreConnection> Connect()
        {
            var connectionSettings = ConnectionSettings.Create()
                .KeepReconnecting()
                .UseDebugLogger();
            var ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
            var endPoint = new IPEndPoint(ipAddress, 1113);
            var conn = EventStoreConnection.Create(connectionSettings, endPoint);
            await conn.ConnectAsync();

            return conn;
        }

        public void Dispose()
        {
            if (_hasConnectionBegun)
                _conn.Value.Dispose();
        }
    }
}