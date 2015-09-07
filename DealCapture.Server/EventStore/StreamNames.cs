using System;

namespace DealCapture.Server.EventStore
{
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