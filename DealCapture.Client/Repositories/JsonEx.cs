using Newtonsoft.Json;

namespace DealCapture.Client.Repositories
{
    public static class JsonEx
    {
        public static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
