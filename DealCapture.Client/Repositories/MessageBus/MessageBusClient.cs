using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DealCapture.Client.Repositories.MessageBus
{
    public interface IMessageBusClient
    {
        Task Enqueue(string queueName, string message);
        Task<string> Dequeue(string queueName);
    }

    public static class MessageBusClientExtensions
    {
        public static Task Enqueue<T>(this IMessageBusClient client, T message)
        {
            var payload = message.ToJson();
            return client.Enqueue(message.GetType().Name, payload);
        }

        public static async Task<T> Dequeue<T>(this IMessageBusClient client)
        {
            var payload = await client.Dequeue(typeof(T).Name);
            return payload.FromJson<T>();
        }
    }


    class MessageBusClient : IMessageBusClient, IDisposable
    {
        private readonly Dictionary<string, BlockingCollection<string>> _queues = new Dictionary<string, BlockingCollection<string>>();
        private readonly object _gate = new object();

        public Task Enqueue(string queueName, string payload)
        {
            return Task.Factory.StartNew(() =>
                {
                    var queue = GetOrCreateQueue(queueName);
                    queue.Add(payload);
                });
        }

        public Task<string> Dequeue(string queueName)
        {
            return Task.Factory.StartNew(() =>
            {
                var queue = GetOrCreateQueue(queueName);
                var consumer = queue.GetConsumingEnumerable();
                return consumer.First();
            });
        }

        private BlockingCollection<string> GetOrCreateQueue(string queueName)
        {
            BlockingCollection<string> queue;
            lock (_gate)
            {
                if (!_queues.TryGetValue(queueName, out queue))
                {
                    queue = new BlockingCollection<string>();
                    _queues[queueName] = queue;
                }
            }
            return queue;
        }

        public void Dispose()
        {
            lock (_gate)
            {
                foreach (var queue in _queues.Values)
                {
                    queue.Dispose();
                }
                _queues.Clear();
            }
        }
    }
}
