using Newtonsoft.Json;
using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages
{
    public class EventMessage<T>
    {
        public EventMessage(Guid deviceId, string eventName, Func<T> valueGenerator)
        {
            ObjectId = deviceId.ToString();
            Model = "abb.ability.device";
            Timestamp = DateTime.UtcNow.ToString("o");
            Value = valueGenerator();
            Event = eventName;
        }

        [JsonProperty("objectId")]
        public string ObjectId { get; private set; }

        [JsonProperty("model")]
        public string Model { get; private set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; private set; }

        [JsonProperty("value")]
        public T Value { get; private set; }

        [JsonProperty("event")]
        public string Event { get; private set; }
    }
}
