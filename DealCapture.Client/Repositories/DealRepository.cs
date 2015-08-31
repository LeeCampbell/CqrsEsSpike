using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using DealCapture.Client.Repositories.Dto;
using DealCapture.Client.Repositories.EventStore;
using DealCapture.Client.Repositories.MessageBus;

namespace DealCapture.Client.Repositories
{
    public interface IDealRepository
    {
        IObservable<Unit> CreateDeal(CreateDealCommand command);
        IObservable<DealRowViewModel> GetAllDealUpdates();
        IObservable<DealRowViewModel> GetDealUpdates(Guid dealId);
    }

    //Naive implementation
    public class DealRepository : IDealRepository
    {
        private readonly IEventStoreClient _eventStoreClient;
        private readonly IMessageBusClient _messageBusClient;

        public DealRepository(IEventStoreClient eventStoreClient, IMessageBusClient messageBusClient)
        {
            _eventStoreClient = eventStoreClient;
            _messageBusClient = messageBusClient;
        }

        public IObservable<Unit> CreateDeal(CreateDealCommand command)
        {
            return Observable.Create<Unit>(
                obs =>
                {
                    var subscription = GetDealUpdates(command.DealId)
                        .Select(_ => Unit.Default)
                        .Take(1)
                        .Timeout(TimeSpan.FromSeconds(5), Scheduler.Default)
                        .Subscribe(obs);
                    
                    //Send the command
                    _messageBusClient.Enqueue(command);

                    return subscription;
                });
        }

        public IObservable<DealRowViewModel> GetAllDealUpdates()
        {
            //Will read all events from the system and filter non-deal streams.
            var allDealEvents = _eventStoreClient.AllEvents()
                .Where(re => StreamNames.IsDealStream(re.Event.EventStreamId));
            //-Or-
            //--Requires the system Category projection to be started. hre $ce-* is a logical CategoryEvent system stream of anything starting with "Deal-"
            //var allDealEvents = _eventStoreClient.GetEvents("$ce-Deal");

            return allDealEvents
                .GroupBy(re => re.Event.EventStreamId)
                .SelectMany(dealStream => dealStream.Select(re => re.Event.Data)
                    .Select(Encoding.UTF8.GetString)
                    .Scan<string, DealRowViewModel>(null, AccumlateDealEvents));
        }

        public IObservable<DealRowViewModel> GetDealUpdates(Guid dealId)
        {
            //Subscribe to the EventStream looking for the DealId
            var streamName = StreamNames.DealStreamName(dealId);
            return _eventStoreClient.GetEvents(streamName)
                .Select(re=>re.OriginalEvent.Data)
                .Select(Encoding.UTF8.GetString)
                .Scan<string, DealRowViewModel>(null, AccumlateDealEvents);
        }

        private static DealRowViewModel AccumlateDealEvents(DealRowViewModel currentState, string eventPayload)
        {
            var evt = eventPayload.FromJson<CreateDealCommand>();
            if (currentState == null)
            {
                return new DealRowViewModel(evt.DealId)
                {
                    Counterparty = evt.Counterparty,
                    Trader = evt.Trader,
                    Status = "Requested",

                    Underlying = evt.Sections[0].ProductType,
                    Notional = evt.Sections[0].Notional,
                    DeliveryStart = evt.Sections[0].DeliveryFrom,
                    DeliveryEnd = evt.Sections[0].DeliverUntil,
                    PercentageFixed = 0
                };
            }

            //Not actually supported, yet.

            var row = new DealRowViewModel(evt.DealId)
            {
                Counterparty = currentState.Counterparty,
                Trader = currentState.Trader,
                Status = currentState.Status,

                Underlying = currentState.Underlying,
                Notional = currentState.Notional,
                DeliveryStart = currentState.DeliveryStart,
                DeliveryEnd = currentState.DeliveryEnd,
                PercentageFixed = currentState.PercentageFixed
            };


            row.Counterparty = evt.Counterparty;
            row.Trader = evt.Trader;
            //row.Status = "Requested";
            row.Underlying = evt.Sections[0].ProductType;
            row.Notional = evt.Sections[0].Notional;
            row.DeliveryStart = evt.Sections[0].DeliveryFrom;
            row.DeliveryEnd = evt.Sections[0].DeliverUntil;
            row.PercentageFixed = 0;

            return row;
        }
    }
}